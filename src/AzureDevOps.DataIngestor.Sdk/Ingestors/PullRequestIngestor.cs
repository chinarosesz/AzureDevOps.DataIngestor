﻿using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using EntityFrameworkCore.BulkOperations;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public class PullRequestIngestor : BaseIngestor
    {
        private readonly ILogger logger;
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly IEnumerable<string> projectNames;

        public PullRequestIngestor(VssClient vssClient, string sqlConnectionString, IEnumerable<string> projectNames, ILogger logger)
        {
            this.logger = logger;
            this.vssClient = vssClient;
            this.sqlConnectionString = sqlConnectionString;
            this.projectNames = projectNames;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync(this.projectNames);

            // For each repositry in each project, retrieve and ingest pull request data
            foreach (TeamProjectReference project in projects)
            {
                this.logger.LogInformation($"Retrieve and ingest {PullRequestStatus.Active} pull requests");
                RetrieveAndIngestPullRequest(project, PullRequestStatus.Active);
                this.logger.LogInformation($"Retrieve and ingest {PullRequestStatus.Completed} pull requests");
                RetrieveAndIngestPullRequest(project, PullRequestStatus.Completed);
            }
        }

        private void RetrieveAndIngestPullRequest(TeamProjectReference project, PullRequestStatus status)
        {
            // Query for most recent date from watermark table first
            DateTime mostRecentDate = this.GetPullRequestWatermark(project, status);

            // Retrieve pull requests from Azure DevOps
            IEnumerable<List<GitPullRequest>> pullRequestLists = this.vssClient.GitClient.GetPullRequestsWithRetry(project.Name, mostRecentDate, status);

            // For each list ingest pull requests retrieved from Azure DevOps
            foreach (List<GitPullRequest> pullRequestList in pullRequestLists)
            {
                this.IngestPullRequests(pullRequestList, project);
            }

            // Update watermark once data is successfully ingested so next time it doesn't repeat
            // Note: If there is an issue before getting to update, data ingestion will have to run again
            // Note: Since we are going back one month ingesting data again if watermark is unable to update
            // Note: shouldn't be much of a problem and likely not happen too often
            this.UpdatePullRequestWatermark(status, project);
        }

        private void IngestPullRequests(List<GitPullRequest> pullRequests, TeamProjectReference project)
        {
            List<VssPullRequestEntity> entities = new List<VssPullRequestEntity>();

            foreach (GitPullRequest pullRequest in pullRequests)
            {
                VssPullRequestEntity entity = new VssPullRequestEntity
                {
                    AuthorEmail = pullRequest.CreatedBy.UniqueName,
                    ClosedDate = pullRequest.ClosedDate,
                    CreationDate = pullRequest.CreationDate,
                    PullRequestId = pullRequest.PullRequestId,
                    LastMergeCommitID = pullRequest.LastMergeCommit?.CommitId,
                    LastMergeTargetCommitId = pullRequest.LastMergeTargetCommit?.CommitId,
                    SourceBranch = pullRequest.SourceRefName,
                    Status = pullRequest.Status.ToString(),
                    TargetBranch = pullRequest.TargetRefName,
                    Title = pullRequest.Title,
                    ProjectId = pullRequest.Repository.ProjectReference.Id,
                    RepositoryId = pullRequest.Repository.Id,
                    Organization = this.vssClient.OrganizationName,
                    RowUpdatedDate = Helper.UtcNow,
                    ProjectName = project.Name,
                };
                entities.Add(entity);
            }

            this.logger.LogInformation("Ingesting pull request data...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            int ingestedResult = context.BulkInsertOrUpdate(entities);
            this.logger.LogInformation($"Done ingesting {ingestedResult} records");
        }

        private void UpdatePullRequestWatermark(PullRequestStatus status, TeamProjectReference project)
        {
            VssPullRequestWatermarkEntity vssPullRequestWatermarkEntity = new VssPullRequestWatermarkEntity
            {
                RowUpdatedDate = Helper.UtcNow,
                Organization = this.vssClient.OrganizationName,
                ProjectId = project.Id,
                ProjectName = project.Name,
                PullRequestStatus = status.ToString(),
            };

            this.logger.LogInformation($"Updating {vssPullRequestWatermarkEntity.PullRequestStatus} pull request watermark...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            int ingestedResult = context.BulkInsertOrUpdate(new List<VssPullRequestWatermarkEntity> { vssPullRequestWatermarkEntity });
            this.logger.LogInformation($"Latest {vssPullRequestWatermarkEntity.PullRequestStatus} pull request watermark at {vssPullRequestWatermarkEntity.RowUpdatedDate}");
        }

        private DateTime GetPullRequestWatermark(TeamProjectReference project, PullRequestStatus status)
        {
            // Default by going back to 1 month if no data has been ingested for this repo
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-1);

            // Get latest ingested date for pull request from pull request watermark table
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            VssPullRequestWatermarkEntity latestWatermark = context.VssPullRequestWatermarkEntities.Where(v => v.ProjectId == project.Id && v.PullRequestStatus == status.ToString()).FirstOrDefault();
            if (latestWatermark != null)
            {
                mostRecentDate = latestWatermark.RowUpdatedDate;
            }

            return mostRecentDate;
        }
    }
}

using AzureDevOps.DataIngestor.Sdk.Clients;
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
    public class CommitIngestor : BaseIngestor
    {
        private readonly ILogger logger;
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly IEnumerable<string> projectNames;

        public CommitIngestor(VssClient vssClient, string sqlConnectionString, IEnumerable<string> projectNames, ILogger logger)
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

            // For each project, retrieve and ingest commit data
            foreach (TeamProjectReference project in projects)
            {
                List<GitRepository> reposFromProject = await this.vssClient.GitClient.GetRepositoriesWithRetryAsync(project.Name);

                foreach (GitRepository repo in reposFromProject)
                {
                    this.logger.LogInformation($"Retrieve and ingest commits");
                    await RetrieveAndIngestCommit(project, repo);
                }
            }
        }

        private async Task RetrieveAndIngestCommit(TeamProjectReference project, GitRepository repo)
        {
            // Query for most recent date from watermark table first
            DateTime mostRecentDate = this.GetCommitWatermark(repo.Id);

            // Retrieve commits from Azure DevOps
            List<GitCommitRef> commitLists = await this.vssClient.GitClient.GetCommitsWithRetryAsync(repo.Id, "", mostRecentDate, DateTime.UtcNow);

            if (commitLists.Count > 0)
            {
                this.IngestCommits(commitLists, repo);
                // Update watermark once data is successfully ingested so next time it doesn't repeat
                // Note: If there is an issue before getting to update, data ingestion will have to run again
                // Note: Since we are going back one month ingesting data again if watermark is unable to update
                // Note: shouldn't be much of a problem and likely not happen too often

                this.UpdateCommitWatermark(project, repo.Id);
            }
        }

        private void IngestCommits(List<GitCommitRef> commits, GitRepository repo)
        {
            List<VssCommitEntity> entities = new List<VssCommitEntity>();

            foreach (GitCommitRef commit in commits)
            {
                VssCommitEntity entity = new VssCommitEntity
                {
                    Organization = this.vssClient.OrganizationName,
                    ProjectName = repo.ProjectReference.Name,
                    ProjectId = repo.ProjectReference.Id,
                    CommitId = commit.CommitId,
                    RepositoryId= repo.Id,
                    AuthorEmail = commit.Author.Email,
                    CommitTime = commit.Author.Date,
                    Comment = commit.Comment,
                    RemoteUrl = commit.RemoteUrl,
                    RowUpdatedDate = Helper.UtcNow,
                };
                entities.Add(entity);
            }

            this.logger.LogInformation("Ingesting commit request data...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            int ingestedResult = context.BulkInsertOrUpdate(entities);
            this.logger.LogInformation($"Done ingesting {ingestedResult} records for Repo Id {repo.Id}");
        }

        private void UpdateCommitWatermark(TeamProjectReference project, Guid repoId)
        {
            VssCommitWatermarkEntity vssCommitWatermarkEntity = new VssCommitWatermarkEntity
            {
                RowUpdatedDate = Helper.UtcNow,
                Organization = this.vssClient.OrganizationName,
                RepositoryId = repoId,
                ProjectId = project.Id,
                ProjectName = project.Name,
            };

            this.logger.LogInformation($"Updating commit watermark...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);

            int ingestedResult = context.BulkInsertOrUpdate(new List<VssCommitWatermarkEntity> { vssCommitWatermarkEntity });
            this.logger.LogInformation($"Latest commit watermark at {vssCommitWatermarkEntity.RowUpdatedDate}");
        }

        private DateTime GetCommitWatermark(Guid repoId)
        {
            // Default by going back to 1 month if no data has been ingested for this repo
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-1);

            // Get latest ingested date for repo from commit watermark table
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            VssCommitWatermarkEntity latestWatermark = context.VssCommitWatermarkEntities.Where(v => v.RepositoryId == repoId).FirstOrDefault();
            if (latestWatermark != null)
            {
                mostRecentDate = latestWatermark.RowUpdatedDate;
            }

            return mostRecentDate;
        }
    }
}

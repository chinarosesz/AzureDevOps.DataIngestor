using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class PullRequestCollector : CollectorBase
    {
        private readonly VssClient vssClient;
        private readonly VssDbContext dbContext;
        private readonly List<string> projectNames;

        public PullRequestCollector(VssClient vssClient, VssDbContext dbContext, List<string> projectNames)
        {
            this.vssClient = vssClient;
            this.dbContext = dbContext;
            this.projectNames = projectNames;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync(this.projectNames);

            // For each repositry in each project, retrieve and ingest pull request data
            foreach (TeamProjectReference project in projects)
            {
                await RetrieveAndIngestPullRequestAsync(project, PullRequestStatus.Active);
                await RetrieveAndIngestPullRequestAsync(project, PullRequestStatus.Completed);
            }
        }

        private async Task RetrieveAndIngestPullRequestAsync(TeamProjectReference project, PullRequestStatus status)
        {
            // Query for most recent date from watermark table first
            DateTime mostRecentDate = this.GetPullRequestWatermark(project, status);

            // Retrieve pull requests from Azure DevOps
            IEnumerable<List<GitPullRequest>> pullRequestLists = this.vssClient.GitClient.GetPullRequestsWithRetry(project.Name, mostRecentDate, status);

            // For each list ingest pull requests retrieved from Azure DevOps
            foreach (List<GitPullRequest> pullRequestList in pullRequestLists)
            {
                await this.IngestPullRequestsAsync(pullRequestList, project);
            }

            // Update watermark once data is successfully ingested so next time it doesn't repeat
            // Note: If there is an issue before getting to update, data ingestion will have to run again
            // Note: Since we are going back one month ingesting data again if watermark is unable to update
            // Note: shouldn't be much of a problem and likely not happen too often
            await this.UpdatePullRequestWatermarkAsync(status, project);
        }

        private async Task IngestPullRequestsAsync(List<GitPullRequest> pullRequests, TeamProjectReference project)
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
                    Data = Helper.SerializeObject(pullRequest),
                };
                entities.Add(entity);
            }

            await this.dbContext.BulkInsertOrUpdateAsync(entities);
        }

        private async Task UpdatePullRequestWatermarkAsync(PullRequestStatus status, TeamProjectReference project)
        {
            VssPullRequestWatermarkEntity vssPullRequestWatermarkEntity = new VssPullRequestWatermarkEntity
            {
                RowUpdatedDate = Helper.UtcNow,
                Organization = this.vssClient.OrganizationName,
                ProjectId = project.Id,
                ProjectName = project.Name,
                PullRequestStatus = status.ToString(),
            };

            await this.dbContext.BulkInsertOrUpdateAsync(new List<VssPullRequestWatermarkEntity> { vssPullRequestWatermarkEntity });
        }

        private DateTime GetPullRequestWatermark(TeamProjectReference project, PullRequestStatus status)
        {
            // Default by going back to 1 month if no data has been ingested for this repo
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-1);

            // Get latest ingested date for pull request from pull request watermark table
            VssPullRequestWatermarkEntity latestWatermark = dbContext.VssPullRequestWatermarkEntities.Where(v => v.ProjectId == project.Id && v.PullRequestStatus == status.ToString()).FirstOrDefault();
            if (latestWatermark != null)
            {
                mostRecentDate = latestWatermark.RowUpdatedDate;
            }

            return mostRecentDate;
        }
    }
}

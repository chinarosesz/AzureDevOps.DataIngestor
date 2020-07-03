using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
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
                await RetrieveAndIngestPullRequestDataAsync(project, PullRequestStatus.Active);
                await RetrieveAndIngestPullRequestDataAsync(project, PullRequestStatus.Completed);
            }
        }

        private async Task RetrieveAndIngestPullRequestDataAsync(TeamProjectReference project, PullRequestStatus status)
        {
            // Query for most recent date from watermark table first
            DateTime mostRecentDate = this.GetMostRecentPullRequestDateFromWatermarkTable(project, status);

            // Retrieve pull requests from Azure DevOps
            IEnumerable<List<GitPullRequest>> pullRequestLists = this.vssClient.GitClient.GetPullRequestsWithRetry(project.Name, mostRecentDate, status);

            // For each list ingest pull request data and update watermark table
            foreach (List<GitPullRequest> pullRequestList in pullRequestLists)
            {
                // For each list convert to DB entities
                List<VssPullRequestEntity> pullRequestEntities = this.ParsePullRequests(pullRequestList, project);

                // Get most recent pull request from current list
                VssPullRequestWatermarkEntity mostRecentPullRequestEntity = this.GetMostRecentPullRequestAndConvertToEntity(pullRequestList, status, project);

                // Insert DB pull requests entities
                using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
                await this.dbContext.BulkInsertOrUpdateAsync(pullRequestEntities);
                await this.dbContext.BulkInsertOrUpdateAsync(new List<VssPullRequestWatermarkEntity>{ mostRecentPullRequestEntity });
                await transaction.CommitAsync();
            }
        }

        private List<VssPullRequestEntity> ParsePullRequests(List<GitPullRequest> pullRequests, TeamProjectReference teamProjectReference)
        {
            List<VssPullRequestEntity> entities = new List<VssPullRequestEntity>();
            foreach (var pullRequest in pullRequests)
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
                    ProjectName = teamProjectReference.Name,
                    Data = Helper.SerializeObject(pullRequest),
                };
                entities.Add(entity);
            }

            return entities;
        }

        private VssPullRequestWatermarkEntity GetMostRecentPullRequestAndConvertToEntity(List<GitPullRequest> gitPullRequests, PullRequestStatus status, TeamProjectReference project)
        {
            GitPullRequest mostRecentPullRequest = null;

            if (status == PullRequestStatus.Completed)
            {
                mostRecentPullRequest = gitPullRequests.Where(v => v.Status == status).OrderByDescending(v => v.ClosedDate).FirstOrDefault();
            }
            else if (status == PullRequestStatus.Active)
            {
                mostRecentPullRequest = gitPullRequests.Where(v => v.Status == status).OrderByDescending(v => v.CreationDate).FirstOrDefault();
            }

            VssPullRequestWatermarkEntity vssPullRequestWatermarkEntity = new VssPullRequestWatermarkEntity
            {
                RowUpdatedDate = Helper.UtcNow,
                Organization = this.vssClient.OrganizationName,
                ProjectId = project.Id,
                ProjectName = project.Name,
                PullRequestStatus = status.ToString(),
            };

            return vssPullRequestWatermarkEntity;
        }

        private DateTime GetMostRecentPullRequestDateFromWatermarkTable(TeamProjectReference project, PullRequestStatus status)
        {
            // Default by going back to 6 months if no data has been ingested for this repo
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-6);

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

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
                List<GitRepository> repositories = await this.vssClient.GitClient.GetRepositoriesWithRetryAsync(project.Name);
                foreach (GitRepository repo in repositories)
                {
                    await RetrieveAndIngestDataAsync(repo, project, PullRequestStatus.Completed);
                }
            }
        }

        private async Task RetrieveAndIngestDataAsync(GitRepository repo, TeamProjectReference project, PullRequestStatus status)
        {
            DateTime mostRecentDate = this.GetMostRecentDateFromDatabase(repo, status);
            IEnumerable<List<GitPullRequest>> pullRequestLists = this.vssClient.GitClient.GetPullRequestsWithRetry(repo, mostRecentDate, status);

            foreach (List<GitPullRequest> pullRequestList in pullRequestLists)
            {
                List<VssPullRequestEntity> pullRequestEntities = this.ParsePullRequests(pullRequestList, project);
                using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
                await this.dbContext.BulkInsertOrUpdateAsync(pullRequestEntities);
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

        private DateTime GetMostRecentDateFromDatabase(GitRepository repo, PullRequestStatus status)
        {
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-6);

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            {
                VssPullRequestEntity mostRecentPr = null;

                if (status == PullRequestStatus.Active)
                {
                    mostRecentPr = dbContext.VssPullRequestEntities.Where(v => v.RepositoryId == repo.Id && v.Status == status.ToString()).OrderByDescending(v => v.CreationDate).FirstOrDefault();

                    if (mostRecentPr != null)
                    {
                        mostRecentDate = mostRecentPr.CreationDate;
                    }
                }
                else if (status == PullRequestStatus.Completed || status == PullRequestStatus.Abandoned)
                {
                    // An abandoned pull request that becomes active may not be in this list because we are using the most recent closed date instead of created date
                    // This is a scenario that we are ok with for now since rarely an abandoned pull request gets reactivated. To fully get an accurate number of active
                    // pull requests, this pump has to be changed to re-pump all active pull requests from the beginning to end so we are ok with not doing that and miss
                    // a few active ones that go from abanoned to active
                    mostRecentPr = dbContext.VssPullRequestEntities.Where(v => v.RepositoryId == repo.Id && v.Status == status.ToString()).OrderByDescending(v => v.ClosedDate).FirstOrDefault();
                    if (mostRecentPr != null)
                    {
                        mostRecentDate = mostRecentPr.ClosedDate;
                    }
                }

                return mostRecentDate;
            }
        }

    }
}

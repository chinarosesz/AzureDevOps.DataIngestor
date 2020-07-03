using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssGitClient : GitHttpClient
    {
        private readonly ILogger logger;
        private readonly TimeSpan retryAfter;

        internal VssGitClient(Uri baseUrl, VssCredentials credentials, ILogger logger) : base(baseUrl, credentials)
        {
            this.logger = logger;
            this.retryAfter = VssClientHelper.GetRetryAfter(this.LastResponseContext);
        }

        public async Task<List<GitCommitRef>> GetCommitsAsync(Guid repositoryId, string branchName, DateTime fromDate, DateTime toDate, int top = 100, int? skip = null)
        {
            GitQueryCommitsCriteria searchCriteria = new GitQueryCommitsCriteria
            {
                FromDate = fromDate.ToUniversalTime().ToString("o"),
                ToDate = toDate.ToUniversalTime().ToString("o"),
                ItemVersion = new GitVersionDescriptor
                {
                    Version = branchName,
                    VersionType = GitVersionType.Branch
                },
            };

            List<GitCommitRef> commitRefs = await RetryHelper.SleepAndRetry(this.retryAfter, this.logger, async () =>
            {
                return await this.GetCommitsAsync(repositoryId, searchCriteria, skip, top);
            });

            return commitRefs;
        }

        public async Task<List<GitRepository>> GetRepositoriesWithRetryAsync(string project)
        {
            List<GitRepository> repos = await RetryHelper.SleepAndRetry(this.retryAfter, this.logger, async () =>
            {
                this.logger.LogInformation($"Retrieving repostiories for project {project}");
                List<GitRepository> repos = await this.GetRepositoriesAsync(project);
                this.logger.LogInformation($"Retrieved {repos.Count} repositories for project {project}");
                return repos;
            });

            return repos;
        }

        /// <summary>
        /// Query all pull requests from today until min creation date where min creation date is in the past (inclusive)
        /// </summary>
        public IEnumerable<List<GitPullRequest>> GetPullRequestsWithRetry(string projectName, DateTime minDate, PullRequestStatus status)
        {
            if (minDate > Helper.UtcNow)
            {
                throw new ArgumentException("minDate must be less than today's date, all in UTC");
            }

            List<GitPullRequest> currentSetOfPullRequests = new List<GitPullRequest>();
            int skip = 0;
            int top = 100;

            GitPullRequestSearchCriteria searchCriteria = new GitPullRequestSearchCriteria
            {
                Status = status,
            };

            do
            {
                this.logger.LogInformation($"Retrieving {status} pull requests for project {projectName} from {minDate} to {Helper.UtcNow}");

                // The last pull request is the where we want to check before stopping
                if (status == PullRequestStatus.Completed && currentSetOfPullRequests.Count > 0 && currentSetOfPullRequests.Last().ClosedDate < minDate)
                {
                    this.logger.LogInformation($"No more pull requests found before {minDate}");
                    break;
                }
                else if (status == PullRequestStatus.Active && currentSetOfPullRequests.Count > 0 && currentSetOfPullRequests.Last().CreationDate < minDate)
                {
                    this.logger.LogInformation($"No more pull requests found before {minDate}");
                    break;
                }
                else if (status == PullRequestStatus.Abandoned && currentSetOfPullRequests.Count > 0 && currentSetOfPullRequests.Last().ClosedDate < minDate)
                {
                    this.logger.LogInformation($"No more pull requests found before {minDate}");
                    break;
                }

                // Get pull requests from VSTS
                currentSetOfPullRequests = RetryHelper.SleepAndRetry(this.retryAfter, this.logger, async () =>
                {
                    try
                    {
                        return await this.GetPullRequestsByProjectAsync(projectName, searchCriteria, skip: skip, top: top);
                    }
                    catch (VssServiceException ex)
                    {
                        // VSTS service fails to access a repo once in a while. It looks like an exception that we
                        // don't have control over so we'll catch it and move on.
                        // Sample exception: Microsoft.VisualStudio.Services.Common.VssServiceException: TF401019: The Git
                        // repository with name or identifier C50B9441-B35B-4F42-BDA9-9A01386B968F does not exist or you
                        // do not have permissions for the operation you are attempting.
                        if (ex.Message.Contains("TF401019"))
                        {
                            Console.WriteLine($"Warning: Ignore this error due to external VSTS service. {ex}");
                        }
                    }
                    
                    return currentSetOfPullRequests;

                }).Result;

                // VSO returns a chunk each time, filter out the ones that meet minDate requirements
                if (status == PullRequestStatus.Completed || status == PullRequestStatus.Abandoned)
                {
                    currentSetOfPullRequests = currentSetOfPullRequests.Where(v => v.ClosedDate > minDate).ToList();
                }
                else if (status == PullRequestStatus.Active)
                {
                    currentSetOfPullRequests = currentSetOfPullRequests.Where(v => v.CreationDate > minDate).ToList();
                }

                // Return a batch of requests at a time
                this.logger.LogInformation($"Retrieved {currentSetOfPullRequests.Count} pull requests");
                yield return currentSetOfPullRequests;

                // Next set
                skip = skip + top;
            }
            while (currentSetOfPullRequests.Count > 0);
        }
    }
}

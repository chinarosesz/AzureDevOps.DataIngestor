using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
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

        public async Task<List<GitRepository>> GetReposAsync(string project)
        {
            List<GitRepository> repos = await RetryHelper.SleepAndRetry(this.retryAfter, this.logger, async () =>
            {
                this.logger.LogInformation($"Retrieving repostiories for project {project}");
                return await this.GetRepositoriesAsync(project);
            });

            return repos;
        }
    }
}

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssGitClient : GitHttpClient
    {
        private TimeSpan RetryAfter
        {
            get
            {
                TimeSpan retryAfter = this.LastResponseContext?.Headers.RetryAfter?.Delta.Value ?? TimeSpan.Zero;
                return retryAfter;
            }
        }

        internal VssGitClient(Uri baseUrl, VssCredentials credentials) : base(baseUrl, credentials)
        {
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

            List<GitCommitRef> commitRefs = await RetryHelper.SleepAndRetry(this.RetryAfter, async () =>

            {
                return await this.GetCommitsAsync(repositoryId, searchCriteria, skip, top);
            });

            return commitRefs;
        }

        public async Task<List<GitRepository>> GetReposAsync(string project)
        {
            List<GitRepository> repos = await RetryHelper.SleepAndRetry(this.RetryAfter, async () =>
            {
                Logger.WriteLine($"Retrieving repostiories for project {project}");
                return await this.GetRepositoriesAsync(project);
            });

            return repos;
        }
    }
}

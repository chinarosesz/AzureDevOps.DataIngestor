using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureDevOpsGitHttpClient : GitHttpClient
    {
        private int RetryAfter;
        public Task<string> CurrentResponseContent { get; private set; }
        public HttpResponseMessage CurrentHttpResponseMessage { get; private set; }

        public AzureDevOpsGitHttpClient(Uri baseUrl, HttpMessageHandler pipeline, bool disposeHandler) : base(baseUrl, pipeline, disposeHandler)
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

            List<GitCommitRef> commitRefs = await RetryHelper.WhenVssException(async () =>
            {
                return await this.GetCommitsAsync(repositoryId, searchCriteria, skip, top);
            });

            return commitRefs;
        }

        public async Task<List<GitRepository>> GetRepositoriesWithRetryAsync(string project)
        {
            List<GitRepository> repos = await RetryHelper.WhenVssException(async () =>
            {
                Logger.WriteLine($"Retrieving repositories for project {project}...");
                List<GitRepository> repos = await this.GetRepositoriesAsync(project);
                Logger.WriteLine($"Retrieved {repos.Count} repositories successfully");
                return repos;
            });

            return repos;
        }

        public async Task<List<GitRepository>> GetRepos(string project)
        {

            List<GitRepository> repos = await this.GetRepositoriesAsync(project);
            RetryConditionHeaderValue retryAfter = this.CurrentHttpResponseMessage.Headers.RetryAfter;
            Logger.WriteLine($"Getting repositories with retryAfter {retryAfter?.Delta.Value} and status code {this.CurrentHttpResponseMessage.StatusCode}");



            if (retryAfter != null)
            {
                repos = await RetryHelper.SleepAndRetry(5, retryAfter.Delta.Value, async () =>
                {
                    Logger.WriteLine($"Getting repositories with retryAfter {retryAfter.Delta.Value} and status code {this.CurrentHttpResponseMessage.StatusCode}");
                    return await this.GetRepositoriesAsync(project);
                });
            }

            return repos;
        }

        protected override Task<T> ReadJsonContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            // Save the current response object
            this.CurrentHttpResponseMessage = response;

            // Save current response content
            this.CurrentResponseContent = response.Content.ReadAsStringAsync();

            return base.ReadJsonContentAsync<T>(response, cancellationToken);
        }
    }
}

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureDevOpsGitHttpClient : GitHttpClient
    {
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

            List<GitCommitRef> commitRefs = await Policy.Handle<VssException>().WaitAndRetryAsync(3, sleepDurations => TimeSpan.FromMinutes(sleepDurations * 5)).ExecuteAsync(async () =>
            {
                return await this.GetCommitsAsync(repositoryId, searchCriteria, skip, top);
            });

            return commitRefs;
        }

        public async Task<List<GitRepository>> GetRepositoriesWithRetryAsync(string project)
        {
            List<GitRepository> repos = await Policy.Handle<VssException>().WaitAndRetryAsync(3, sleepDurations => TimeSpan.FromSeconds(sleepDurations * 2)).ExecuteAsync(async () =>
            {
                Logger.WriteLine($"Retrieving repositories for project {project}...");
                List<GitRepository> repos = await this.GetRepositoriesAsync(project);
                Logger.WriteLine($"Retrieved {repos.Count} repositories successfully");
                return repos;
            });

            return repos;
        }

        protected override Task<T> ReadJsonContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            this.CurrentHttpResponseMessage = response;
            this.CurrentResponseContent = response.Content.ReadAsStringAsync();
            return base.ReadJsonContentAsync<T>(response, cancellationToken);
        }
    }
}

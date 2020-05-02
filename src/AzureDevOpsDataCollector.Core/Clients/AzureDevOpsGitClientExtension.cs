using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public static class AzureDevOpsGitClientExtension
    {
        public static async Task<List<GitCommitRef>> GetCommitsAsync(this GitHttpClient client, Guid repositoryId, string branchName, DateTime fromDate, DateTime toDate, int top = 100, int? skip = null)
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
                return await client.GetCommitsAsync(repositoryId, searchCriteria, skip, top);
            });

            return commitRefs;
        }

        public static async Task<List<GitRepository>> GetRepositoriesWithRetryAsync(this GitHttpClient client, string project)
        {
            List<GitRepository> repos = await Policy.Handle<VssException>().WaitAndRetryAsync(3, sleepDurations => TimeSpan.FromSeconds(sleepDurations * 2)).ExecuteAsync(async () =>
            {
                Logger.WriteLine($"Retrieving repositories for project {project}...");
                List<GitRepository> repos = await client.GetRepositoriesAsync(project);
                Logger.WriteLine($"Retrieved {repos.Count} repositories successfully");
                return repos;
            });

            return repos;
        }
    }
}

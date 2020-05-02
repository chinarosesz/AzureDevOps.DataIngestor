using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class RepositoryCollector : CollectorBase
    {
        private IEnumerable<string> projects;

        public RepositoryCollector(AzureDevOpsClient azureDevOpsClient, AzureDevOpsDbContext dbContext, IEnumerable<string> projects) : base(azureDevOpsClient, dbContext)
        {
            this.projects = projects;
        }

        public async Task RunAsync()
        {
            GitHttpClient gitHttpClient = await this.azureDevOpsClient.VssConnection.GetClientAsync<GitHttpClient>();

            foreach (string project in this.projects)
            {
                this.DisplayProjectHeader(project);
                List<GitRepository> repos = await gitHttpClient.GetRepositoriesWithRetryAsync(project);
                await this.InsertOrUpdateRepositories(repos);
            }
        }

        private async Task InsertOrUpdateRepositories(List<GitRepository> repositories)
        {
            List<RepositoryEntity> repoEntities = new List<RepositoryEntity>();

            foreach (GitRepository repo in repositories)
            {
                RepositoryEntity repoEntity = new RepositoryEntity
                {
                    OrganizationName = this.azureDevOpsClient.OrganizationName,
                    Id = repo.Id,
                    RepoName = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    ProjectId = repo.ProjectReference.Id,
                    ProjectName = repo.ProjectReference.Name,
                    RemoteUrl = repo.RemoteUrl,
                    RepoUrl = repo.Url,
                    RowUpdatedDate = this.Now,
                };
                repoEntities.Add(repoEntity);
            }

            await this.dbContext.BulkInsertOrUpdateAsync(repoEntities);
        }
    }
}

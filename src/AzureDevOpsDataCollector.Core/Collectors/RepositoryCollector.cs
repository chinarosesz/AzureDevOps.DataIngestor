using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class RepositoryCollector : CollectorBase
    {
        private IEnumerable<string> projectNames;
        AzureDevOpsGitHttpClient gitHttpClient;

        public RepositoryCollector(AzureDevOpsClient azureDevOpsClient, AzureDevOpsDbContext dbContext, IEnumerable<string> projectNames) : base(azureDevOpsClient, dbContext)
        {
            this.projectNames = projectNames;
        }

        public async Task RunAsync()
        {
            AzureDevOpsProjectHttpClient projectHttpClient = await this.azureDevOpsClient.VssConnection.GetClientAsync<AzureDevOpsProjectHttpClient>();
            if (projectNames.IsNullOrEmpty())
            {
                this.projectNames = await projectHttpClient.GetProjectNamesWithRetryAsync();
            }

            gitHttpClient = await this.azureDevOpsClient.VssConnection.GetClientAsync<AzureDevOpsGitHttpClient>();
            foreach (string projectName in this.projectNames)
            {
                this.DisplayProjectHeader(projectName);
                List<GitRepository> repos = await gitHttpClient.GetRepositoriesWithRetryAsync(projectName);
                await this.InsertOrUpdateRepositories(repos, projectName);
            }
        }

        private async Task InsertOrUpdateRepositories(List<GitRepository> repositories, string projectName)
        {
            List<RepositoryEntity> repoEntities = new List<RepositoryEntity>();

            foreach (GitRepository repo in repositories)
            {
                RepositoryEntity repoEntity = new RepositoryEntity
                {
                    OrganizationName = this.azureDevOpsClient.OrganizationName,
                    RepoId = repo.Id,
                    RepoName = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    ProjectId = repo.ProjectReference.Id,
                    ProjectName = repo.ProjectReference.Name,
                    WebUrl = repo.RemoteUrl,
                    RequestUrl = this.gitHttpClient.CurrentHttpResponseMessage.RequestMessage.RequestUri.ToString(),
                    RowUpdatedDate = this.Now,
                };
                repoEntities.Add(repoEntity);
            }

            RequestEntity requestEntity = new RequestEntity
            {
                RequestUrl = this.gitHttpClient.CurrentHttpResponseMessage.RequestMessage.RequestUri.ToString(),
                ResponseContent = await this.gitHttpClient.CurrentResponseContent,
                OrganizationName = this.azureDevOpsClient.OrganizationName,
                ProjectName = projectName,
                RowUpdatedDate = this.Now,
            };

            using (IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction())
            {
                await this.dbContext.BulkInsertOrUpdateAsync(repoEntities);
                await this.dbContext.BulkInsertOrUpdateAsync(new List<RequestEntity> { requestEntity });
                transaction.Commit();
            }
        }
    }
}

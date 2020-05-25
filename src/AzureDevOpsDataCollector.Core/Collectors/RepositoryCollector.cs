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

        public RepositoryCollector(VssClientConnector vssClientConnector, AzureDevOpsDbContext dbContext, IEnumerable<string> projectNames) : base(vssClientConnector, dbContext)
        {
            this.projectNames = projectNames;
        }

        public async Task RunAsync()
        {
            // Get projects
            if (projectNames.IsNullOrEmpty())
            {
                this.projectNames = await this.vssClientConnector.ProjectClient.GetProjectNamesAsync();
            }

            // Get repos
            foreach (string projectName in this.projectNames)
            {
                this.DisplayProjectHeader(projectName);
                var repos = await this.vssClientConnector.GitClient.GetReposAsync(projectName);
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
                    OrganizationName = this.vssClientConnector.OrganizationName,
                    RepoId = repo.Id,
                    RepoName = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    ProjectId = repo.ProjectReference.Id,
                    ProjectName = repo.ProjectReference.Name,
                    WebUrl = repo.RemoteUrl,
                    RequestUrl = this.vssClientConnector.GitClient.HttpContext.RequestUri.ToString(),
                    RowUpdatedDate = this.Now,
                };
                repoEntities.Add(repoEntity);
            }

            RequestEntity requestEntity = new RequestEntity
            {
                RequestUrl = this.vssClientConnector.GitClient.HttpContext.RequestUri.ToString(),
                ResponseContent = await this.vssClientConnector.GitClient.HttpContext.ResponseContent,
                OrganizationName = this.vssClientConnector.OrganizationName,
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

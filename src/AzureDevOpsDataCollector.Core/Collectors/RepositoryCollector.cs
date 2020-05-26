using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class RepositoryCollector : CollectorBase
    {
        private readonly VssClientConnector vssClientConnector;
        private readonly AzureDevOpsDbContext dbContext;
        private readonly IEnumerable<string> projectNames;
        private IEnumerable<TeamProjectReference> projects;

        public RepositoryCollector(VssClientConnector vssClientConnector, AzureDevOpsDbContext dbContext, IEnumerable<string> projectNames)
        {
            this.vssClientConnector = vssClientConnector;
            this.dbContext = dbContext;
            this.projectNames = projectNames;
        }

        public async Task RunAsync()
        {
            // Get projects
            this.projects = await this.vssClientConnector.ProjectClient.GetProjectNamesAsync(this.projectNames);

            // Get repos
            foreach (TeamProjectReference project in this.projects)
            {
                this.DisplayProjectHeader(project.Name);
                List<GitRepository> repos = await this.vssClientConnector.GitClient.GetReposAsync(project.Name);
                await this.InsertOrUpdateRepositories(repos, project.Name);
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
                    RequestUrl = this.vssClientConnector.GitClient.VssHttpContext.RequestUri.ToString(),
                    RowUpdatedDate = this.Now,
                };
                repoEntities.Add(repoEntity);
            }

            RequestEntity requestEntity = new RequestEntity
            {
                RequestUrl = this.vssClientConnector.GitClient.VssHttpContext.RequestUri.ToString(),
                ResponseContent = this.vssClientConnector.GitClient.VssHttpContext.ResponseContent,
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

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
        private readonly VssDbContext dbContext;
        private readonly IEnumerable<string> projectNames;
        private IEnumerable<TeamProjectReference> projects;

        public RepositoryCollector(VssClientConnector vssClientConnector, VssDbContext dbContext, IEnumerable<string> projectNames)
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
            List<VssRepositoryEntity> repoEntities = new List<VssRepositoryEntity>();

            foreach (GitRepository repo in repositories)
            {
                VssRepositoryEntity repoEntity = new VssRepositoryEntity
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

            VssRequestEntity requestEntity = new VssRequestEntity
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
                await this.dbContext.BulkInsertOrUpdateAsync(new List<VssRequestEntity> { requestEntity });
                transaction.Commit();
            }
        }
    }
}

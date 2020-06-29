using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class RepositoryCollector : CollectorBase
    {
        private readonly VssClient vssClient;
        private readonly VssDbContext dbContext;
        private readonly IEnumerable<string> projectNames;
        private IEnumerable<TeamProjectReference> projects;
        private readonly ILogger logger;

        public RepositoryCollector(VssClient vssClient, VssDbContext dbContext, IEnumerable<string> projectNames, ILogger logger)
        {
            this.vssClient = vssClient;
            this.dbContext = dbContext;
            this.projectNames = projectNames;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects
            this.projects = await this.vssClient.ProjectClient.GetProjectNamesAsync(this.projectNames);

            // Get repos for all projects
            List<GitRepository> repositories = new List<GitRepository>();
            foreach (TeamProjectReference project in this.projects)
            {
                CollectorHelper.DisplayProjectHeader(this, project.Name, this.logger);
                List<GitRepository> reposFromProject = await this.vssClient.GitClient.GetReposAsync(project.Name);
                repositories.AddRange(reposFromProject);
            }

            // Insert repos
            await this.IngestData(repositories);
        }

        private async Task IngestData(List<GitRepository> repositories)
        {
            List<VssRepositoryEntity> entities = new List<VssRepositoryEntity>();
            
            foreach (GitRepository repo in repositories)
            {
                VssRepositoryEntity repoEntity = new VssRepositoryEntity
                {
                    Organization = this.vssClient.OrganizationName,
                    RepoId = repo.Id,
                    Name = repo.Name,
                    DefaultBranch = repo.DefaultBranch,
                    ProjectId = repo.ProjectReference.Id,
                    ProjectName = repo.ProjectReference.Name,
                    WebUrl = repo.RemoteUrl,
                    Data = CollectorHelper.SerializeObject(repo),
                };
                entities.Add(repoEntity);
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkDeleteAsync(this.dbContext.VssRepositoryEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null).ToList());
            await this.dbContext.BulkInsertAsync(entities);
            await transaction.CommitAsync();
        }
    }
}

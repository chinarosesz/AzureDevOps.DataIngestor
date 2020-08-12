using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using EntityFramework.BulkOperations;
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
        private readonly string sqlConnectionString;
        private readonly IEnumerable<string> projectNames;
        private readonly ILogger logger;

        public RepositoryCollector(VssClient vssClient, string sqlConnectionString, IEnumerable<string> projectNames, ILogger logger)
        {
            this.vssClient = vssClient;
            this.sqlConnectionString = sqlConnectionString;
            this.projectNames = projectNames;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync(this.projectNames);

            // Get repos for all projects
            List<GitRepository> repositories = new List<GitRepository>();
            foreach (TeamProjectReference project in projects)
            {
                this.DisplayProjectHeader(this.logger, project.Name);
                List<GitRepository> reposFromProject = await this.vssClient.GitClient.GetRepositoriesWithRetryAsync(project.Name);
                repositories.AddRange(reposFromProject);
            }

            // Insert repos
            this.IngestData(repositories);
        }

        private void IngestData(List<GitRepository> repositories)
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
                    Data = Helper.SerializeObject(repo),
                };
                entities.Add(repoEntity);
            }

            using VssDbContext context = new VssDbContext(this.sqlConnectionString, logger);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            context.BulkDelete(context.VssRepositoryEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null).ToList());
            context.BulkInsert(entities);
            transaction.Commit();
        }
    }
}

using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using EntityFramework.BulkOperations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public class RepositoryIngestor : BaseIngestor
    {
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly IEnumerable<string> projectNames;
        private readonly ILogger logger;

        public RepositoryIngestor(VssClient vssClient, string sqlConnectionString, IEnumerable<string> projectNames, ILogger logger)
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
                };
                entities.Add(repoEntity);
            }

            this.logger.LogInformation("Start ingesting repsoitories data to database...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            context.BulkDelete(context.VssRepositoryEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null));
            int insertResult = context.BulkInsert(entities);
            transaction.Commit();
            this.logger.LogInformation($"Done ingesting {insertResult} records");
        }
    }
}

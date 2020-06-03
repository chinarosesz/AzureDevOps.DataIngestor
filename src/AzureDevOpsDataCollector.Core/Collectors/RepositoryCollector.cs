using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class RepositoryCollector : CollectorBase
    {
        private readonly VssClient vssClientConnector;
        private readonly VssDbContext dbContext;
        private readonly IEnumerable<string> projectNames;
        private IEnumerable<TeamProjectReference> projects;

        public RepositoryCollector(VssClient vssClient, VssDbContext dbContext, IEnumerable<string> projectNames)
        {
            this.vssClientConnector = vssClient;
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
                await this.InsertOrUpdateRepositories(repos);
            }
        }

        private async Task InsertOrUpdateRepositories(List<GitRepository> repositories)
        {
            List<VssRepositoryEntity> repoEntities = new List<VssRepositoryEntity>();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

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
                    Data = JsonConvert.SerializeObject(repo, jsonSerializerSettings),
                };
                repoEntities.Add(repoEntity);
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkInsertOrUpdateAsync(repoEntities);
            transaction.Commit();
        }
    }
}

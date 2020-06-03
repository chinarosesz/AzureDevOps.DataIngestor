using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Core.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class ProjectCollector : CollectorBase
    {
        private readonly VssClient vssClientConnector;
        private readonly VssDbContext dbContext;
        private readonly IEnumerable<string> projectNames;

        public ProjectCollector(VssClient vssClient, VssDbContext dbContext, IEnumerable<string> projectNames)
        {
            this.vssClientConnector = vssClient;
            this.dbContext = dbContext;
            this.projectNames = projectNames;
        }

        public async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClientConnector.ProjectClient.GetProjectsAsync();

            // Insert or update projects
            await this.InsertOrUpdateRepositories(projects);
        }

        private async Task InsertOrUpdateRepositories(List<TeamProjectReference> projects)
        {
            List<VssProjectEntity> entities = new List<VssProjectEntity>();
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            foreach (TeamProjectReference project in projects)
            {
                VssProjectEntity entity = new VssProjectEntity
                {
                    OrganizationName = this.vssClientConnector.OrganizationName,
                    ProjectId = project.Id,
                    Name = project.Name,
                    LastUpdateTime = project.LastUpdateTime,
                    Revision = project.Revision,
                    State = project.State.ToString(),
                    Visibility = project.Visibility.ToString(),
                    Url = project.Url,
                    Data = JsonConvert.SerializeObject(project, jsonSerializerSettings),
                };
                entities.Add(entity);
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkInsertOrUpdateAsync(entities);
            transaction.Commit();
        }
    }
}

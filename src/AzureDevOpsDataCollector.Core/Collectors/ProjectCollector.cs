using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class ProjectCollector : CollectorBase
    {
        private readonly VssClient vssClientConnector;
        private readonly VssDbContext dbContext;
        private readonly ILogger logger;

        public ProjectCollector(VssClient vssClient, VssDbContext dbContext, ILogger logger)
        {
            this.vssClientConnector = vssClient;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClientConnector.ProjectClient.GetProjectsAsync();

            // Insert or update projects
            await this.InsertOrUpdateProjects(projects);
        }

        private async Task InsertOrUpdateProjects(List<TeamProjectReference> projects)
        {
            List<VssProjectEntity> entities = new List<VssProjectEntity>();
            
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
                    Data = CollectorHelper.SerializeObject(project),
                };
                entities.Add(entity);
            }

            this.logger.LogInformation("Getting ready for database work");
            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            this.logger.LogInformation("Beginning transaction");
            this.dbContext.BulkInsertOrUpdate(entities);
            await this.dbContext.BulkInsertOrUpdateAsync(entities);
            transaction.Commit();
        }
    }
}

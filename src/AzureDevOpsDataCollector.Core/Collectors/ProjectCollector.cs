using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class ProjectCollector
    {
        private readonly VssClient vssClientConnector;
        private readonly VssDbContext dbContext;

        public ProjectCollector(VssClient vssClient, VssDbContext dbContext)
        {
            this.vssClientConnector = vssClient;
            this.dbContext = dbContext;
        }

        public async Task RunAsync()
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

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkInsertOrUpdateAsync(entities);
            transaction.Commit();
        }
    }
}

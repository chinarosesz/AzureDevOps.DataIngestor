using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class ProjectCollector : CollectorBase
    {
        private readonly VssClient vssClient;
        private readonly VssDbContext dbContext;

        public ProjectCollector(VssClient vssClient, VssDbContext dbContext)
        {
            this.vssClient = vssClient;
            this.dbContext = dbContext;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync();

            // Insert or update projects
            await this.IngestData(projects);
        }

        private async Task IngestData(List<TeamProjectReference> projects)
        {
            List<VssProjectEntity> entities = new List<VssProjectEntity>();
            
            foreach (TeamProjectReference project in projects)
            {
                VssProjectEntity entity = new VssProjectEntity
                {
                    Organization = this.vssClient.OrganizationName,
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
            await this.dbContext.BulkDeleteAsync(this.dbContext.VssProjectEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null).ToList());
            await this.dbContext.BulkInsertAsync(entities);
            await transaction.CommitAsync();
        }
    }
}

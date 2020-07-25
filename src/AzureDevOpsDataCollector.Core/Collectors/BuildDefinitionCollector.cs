using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class BuildDefinitionCollector : CollectorBase
    {
        private readonly VssClient vssClient;
        private readonly VssDbContext dbContext;
        private readonly ILogger logger;

        public BuildDefinitionCollector(VssClient vssClient, VssDbContext dbContext, ILogger logger)
        {
            this.vssClient = vssClient;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync();

            // Get build definitions for each project from Azure DevOps and ingest data into SQL Server
            foreach (TeamProjectReference project in projects)
            {
                Helper.DisplayProjectHeader(this, project.Name, this.logger);
                List<BuildDefinition> buildDefinitions = await this.vssClient.BuildClient.GetFullBuildDefinitionsWithRetryAsync(project.Name);
                await this.IngestData(buildDefinitions, project);
            }
        }

        private async Task IngestData(List<BuildDefinition> buildDefinitions, TeamProjectReference project)
        {
            List<VssBuildDefinitionEntity> entities = new List<VssBuildDefinitionEntity>();
            
            foreach (BuildDefinition buildDefinition in buildDefinitions)
            {
                VssBuildDefinitionEntity entity = new VssBuildDefinitionEntity
                {
                    Id = buildDefinition.Id,
                    Name = buildDefinition.Name,
                    Path = buildDefinition.Path,
                    ProjectName = buildDefinition.Project.Name,
                    ProjectId = buildDefinition.Project.Id,
                    PoolName = buildDefinition.Queue.Pool.Name,
                    PoolId = buildDefinition.Queue.Pool.Id,
                    IsHosted = buildDefinition.Queue.Pool.IsHosted,
                    QueueName = buildDefinition.Queue.Name,
                    QueueId = buildDefinition.Queue.Id,
                    CreatedDate = buildDefinition.CreatedDate,
                    UniqueName = buildDefinition.AuthoredBy.UniqueName,
                    Process = buildDefinition.Process.GetType().Name,
                    WebLink = (buildDefinition.Links.Links["web"] as ReferenceLink).Href,
                    Organization = this.vssClient.OrganizationName,
                    Data = Helper.SerializeObject(buildDefinition),
                };
                entities.Add(entity);
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkDeleteAsync(this.dbContext.VssBuildDefinitionEntities.Where(v => v.Organization == this.vssClient.OrganizationName && v.ProjectId == project.Id).ToList());
            await this.dbContext.BulkInsertAsync(entities);
            await transaction.CommitAsync();
        }
    }
}

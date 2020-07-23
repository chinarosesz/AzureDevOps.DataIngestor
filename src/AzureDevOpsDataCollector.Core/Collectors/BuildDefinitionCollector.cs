using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
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
                List<BuildDefinitionReference> buildDefinitions = await this.vssClient.BuildClient.GetBuildDefinitionsAsync(project.Name);
                await this.IngestData(buildDefinitions);
            }
        }

        private async Task IngestData(List<BuildDefinitionReference> buildDefinitionReferences)
        {
            List<VssBuildDefinitionEntity> entities = new List<VssBuildDefinitionEntity>();
            
            foreach (BuildDefinitionReference buildDefinition in buildDefinitionReferences)
            {
                VssBuildDefinitionEntity entity = new VssBuildDefinitionEntity
                {
                    Id = buildDefinition.Id,
                    Name = buildDefinition.Name,
                    Path = buildDefinition.Path,
                    ProjectName = buildDefinition.Project.Name,
                    ProjectId = buildDefinition.Project.Id,
                    PoolName = buildDefinition.Queue.Pool.Name,
                    CreatedDate = buildDefinition.CreatedDate,
                    UniqueName = buildDefinition.AuthoredBy.UniqueName,
                    Organization = this.vssClient.OrganizationName,
                    Data = Helper.SerializeObject(buildDefinition),
                };
                entities.Add(entity);
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkInsertOrUpdateAsync(entities);
            await transaction.CommitAsync();
        }
    }
}

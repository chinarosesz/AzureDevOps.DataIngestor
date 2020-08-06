﻿using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public class ProjectCollector : CollectorBase
    {
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly ILogger logger;

        public ProjectCollector(VssClient vssClient, string sqlConnectionString, ILogger logger)
        {
            this.vssClient = vssClient;
            this.sqlConnectionString = sqlConnectionString;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync();

            // Insert projects
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
                    Data = Helper.SerializeObject(project),
                };
                entities.Add(entity);
            }

            using VssDbContext vssDbContext = new VssDbContext(this.sqlConnectionString, logger);
            using IDbContextTransaction transaction = vssDbContext.Database.BeginTransaction();
            await vssDbContext.BulkDeleteAsync(vssDbContext.VssProjectEntities.Where(v => v.Organization == this.vssClient.OrganizationName || v.Organization == null).ToList());
            await vssDbContext.BulkInsertAsync(entities);
            await transaction.CommitAsync();
        }
    }
}

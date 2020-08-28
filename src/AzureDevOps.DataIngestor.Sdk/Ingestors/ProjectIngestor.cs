using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using EntityFramework.BulkOperations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public class ProjectIngestor : BaseIngestor
    {
        private readonly VssClient vssClient;
        private readonly string sqlConnectionString;
        private readonly ILogger logger;

        public ProjectIngestor(VssClient vssClient, string sqlConnectionString, ILogger logger)
        {
            this.vssClient = vssClient;
            this.sqlConnectionString = sqlConnectionString;
            this.logger = logger;
        }

        public override async Task RunAsync()
        {
            // Get projects from Azure DevOps
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync();

            // Ingest data into database
            this.IngestData(projects);
        }

        private void IngestData(List<TeamProjectReference> projects)
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
                };
                entities.Add(entity);
            }

            this.logger.LogInformation("Start ingesting projects data...");
            using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
            using IDbContextTransaction transaction = context.Database.BeginTransaction();
            context.BulkDelete(context.VssProjectEntities.Where(v => v.Organization == this.vssClient.OrganizationName));
            int insertedResult = context.BulkInsert(entities);
            transaction.Commit();
            this.logger.LogInformation($"Done ingesting {insertedResult} records");
        }
    }
}

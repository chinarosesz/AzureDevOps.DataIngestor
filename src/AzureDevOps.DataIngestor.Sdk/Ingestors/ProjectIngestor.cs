using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using CsvHelper;
using CsvHelper.Configuration;
using EntityFrameworkCore.BulkOperations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

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


            if (Helper.ExtractToCSV)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    // Don't write the header again.
                    HasHeaderRecord = Helper.ExtractToCSVExportHeader,
                    SanitizeForInjection = true
                };

                using (Stream stream = File.Open(@".\csv\Project.csv", Helper.ExtractToCSVExportHeader ? FileMode.Create : FileMode.Append))
                using (CsvWriter csv = new CsvWriter(new StreamWriter(stream), config))
                {
                    csv.WriteRecords(entities);
                    this.logger.LogInformation($"Done exporting projects to CSV file");
                    Helper.ExtractToCSVExportHeader = false;
                }
            }
            else
            {
                using VssDbContext context = new VssDbContext(logger, this.sqlConnectionString);
                using IDbContextTransaction transaction = context.Database.BeginTransaction();
                context.BulkDelete(context.VssProjectEntities.Where(v => v.Organization == this.vssClient.OrganizationName));
                int insertedResult = context.BulkInsert(entities);
                transaction.Commit();
                this.logger.LogInformation($"Done ingesting {insertedResult} records");
            }
        }
    }
}

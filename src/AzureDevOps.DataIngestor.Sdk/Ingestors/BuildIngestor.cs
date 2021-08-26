using AzureDevOps.DataIngestor.Sdk.Clients;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureDevOps.DataIngestor.Sdk.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.BulkOperations;
using System.Linq;
using AzureDevOps.DataIngestor.Sdk.Util;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public class BuildIngestor : BaseIngestor
    {
        private readonly VssClient vssClient;
        private readonly ILogger logger;
        private readonly IEnumerable<string> projectNames;
        private readonly string sqlServerConnectionString;

        public BuildIngestor(VssClient vssClient, string sqlServerConnectionString, IEnumerable<string> projectNames, ILogger logger)
        {
            this.vssClient = vssClient;
            this.logger = logger;
            this.projectNames = projectNames;
            this.sqlServerConnectionString = sqlServerConnectionString;
        }

        public override async Task RunAsync()
        {
            // Get projects
            List<TeamProjectReference> projects = await this.vssClient.ProjectClient.GetProjectsAsync(this.projectNames);

            // Get builds for each project
            foreach (TeamProjectReference project in projects)
            {
                this.DisplayProjectHeader(this.logger, project.Name);
                // Query for most recent date from watermark table first
                DateTime mostRecentDate = this.GetBuildWatermark(project);
                // Retrieve builds from Azure DevOps
                // We set minFinishTime based on the watermark so that only newer builds will be retrieved
                List<Build> buildsFromProject = await this.vssClient.BuildClient.GetBuildsAsync(project.Name, minFinishTime: mostRecentDate);
                this.IngestBuilds(buildsFromProject);
                this.UpdateBuildWatermark(project, buildsFromProject);
            }
        }

        private void IngestBuilds(List<Build> builds)
        {
            List<VssBuildEntity> buildEntities = new List<VssBuildEntity>();
            foreach (Build build in builds)
            {
                VssBuildEntity buildEntity = new VssBuildEntity
                {
                    Id = build.Id,
                    ProjectId = build.Project.Id,
                    Organization = this.vssClient.OrganizationName,
                    RepositoryId = new Guid(build.Repository.Id),
                    BuildNumber = build.BuildNumber,
                    KeepForever = build.KeepForever,
                    RetainedByRelease = build.RetainedByRelease,
                    Status = build.Status,
                    Result = build.Result,
                    QueueTime = build.QueueTime,
                    StartTime = build.StartTime,
                    FinishTime = build.FinishTime,
                    Url = build.Url,
                    DefinitionId = build.Definition.Id,
                    SourceBranch = build.SourceBranch,
                    SourceVersion = build.SourceVersion,
                    QueueId = build.Queue.Id,
                    QueueName = build.Queue.Name,
                };
                buildEntities.Add(buildEntity);
            }

            this.logger.LogInformation("Ingesting build data...");
            using VssDbContext dbContext = new VssDbContext(logger, this.sqlServerConnectionString);
            using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
            int ingestedResult = dbContext.BulkInsertOrUpdate(buildEntities);
            transaction.Commit();
            this.logger.LogInformation($"Done ingesting {ingestedResult} builds");
        }

        private void UpdateBuildWatermark(TeamProjectReference project, List<Build> buildsFromProject)
        {
            List<Build> buildsSorted = buildsFromProject.Where(b => b.FinishTime != null).OrderByDescending(b => b.FinishTime).ToList();
            // latest build time should be the newest ingested build, or if none were newly ingested, stay the same as it was.
            DateTime latestBuildDate = (DateTime)(buildsSorted.Count > 0 ? buildsSorted.First().FinishTime : this.GetBuildWatermark(project));
            VssBuildWatermarkEntity vssBuildWatermarkEntity = new VssBuildWatermarkEntity
            {
                LatestBuildFinishTime = latestBuildDate,
                RowUpdatedDate = Helper.UtcNow,
                Organization = this.vssClient.OrganizationName,
                ProjectId = project.Id,
                ProjectName = project.Name,
            };
            this.logger.LogInformation("Updating build watermark...");
            using VssDbContext context = new VssDbContext(logger, this.sqlServerConnectionString);
            int ingestedResult = context.BulkInsertOrUpdate(new List<VssBuildWatermarkEntity> { vssBuildWatermarkEntity });
            this.logger.LogInformation($"Latest build watermark at {vssBuildWatermarkEntity.RowUpdatedDate}.");
        }

        private DateTime GetBuildWatermark(TeamProjectReference project)
        {

            // Default by going back to 1 month if no data has been ingested for this project
            DateTime mostRecentDate = DateTime.UtcNow.AddMonths(-1);
            
            // Get latest ingested date for build from build watermark table
            using VssDbContext context = new VssDbContext(logger, this.sqlServerConnectionString);
            VssBuildWatermarkEntity latestWatermark = context.VssBuildWatermarkEntities.Where(v => v.ProjectId == project.Id).FirstOrDefault();
            if (latestWatermark != null)
            {
                mostRecentDate = latestWatermark.LatestBuildFinishTime;
            }

            return mostRecentDate;
        }
    }
}

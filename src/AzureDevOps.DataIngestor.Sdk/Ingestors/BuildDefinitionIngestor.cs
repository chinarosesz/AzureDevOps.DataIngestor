﻿using AzureDevOps.DataIngestor.Sdk.Clients;
using AzureDevOps.DataIngestor.Sdk.Entities;
using AzureDevOps.DataIngestor.Sdk.Util;
using EntityFrameworkCore.BulkOperations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public class BuildDefinitionIngestor : BaseIngestor
    {
        private readonly VssClient vssClient;
        private readonly ILogger logger;
        private readonly IEnumerable<string> projectNames;
        private readonly string sqlServerConnectionString;

        public BuildDefinitionIngestor(VssClient vssClient, string sqlServerConnectionString, IEnumerable<string> projectNames, ILogger logger)
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

            // Get build definitions for each project from Azure DevOps and ingest data into SQL Server
            foreach (TeamProjectReference project in projects)
            {
                this.DisplayProjectHeader(this.logger, project.Name);

                // Get build definiitions from Azure DevOps
                IAsyncEnumerable<List<BuildDefinition>> definitionsLists = this.vssClient.BuildClient.GetFullBuildDefinitionsWithRetryAsync(project.Name);
                List<BuildDefinition> buildDefinitions = new List<BuildDefinition>();
                await foreach (List<BuildDefinition> definitionList in definitionsLists)
                {
                    buildDefinitions.AddRange(definitionList);
                    this.IngestData(definitionList, project);
                }
            }

            // Remove build definitions that don't exist in current list
            this.Cleanup();
        }

        private void IngestData(List<BuildDefinition> buildDefinitions, TeamProjectReference project)
        {
            List<VssBuildDefinitionEntity> buildDefinitionEntities = new List<VssBuildDefinitionEntity>();
            List<VssBuildDefinitionStepEntity> buildDefinitionStepEntities = new List<VssBuildDefinitionStepEntity>();

            foreach (BuildDefinition buildDefinition in buildDefinitions)
            {
                // Parse build definitions
                VssBuildDefinitionEntity buildDefinitionEntity = new VssBuildDefinitionEntity
                {
                    Id = buildDefinition.Id,
                    Name = buildDefinition.Name,
                    Path = buildDefinition.Path,
                    ProjectName = buildDefinition.Project.Name,
                    ProjectId = buildDefinition.Project.Id,
                    PoolName = buildDefinition.Queue?.Pool?.Name,
                    PoolId = buildDefinition.Queue?.Pool?.Id,
                    IsHosted = buildDefinition.Queue?.Pool?.IsHosted,
                    QueueName = buildDefinition.Queue?.Name,
                    QueueId = buildDefinition.Queue?.Id,
                    CreatedDate = buildDefinition.CreatedDate,
                    UniqueName = buildDefinition.AuthoredBy.UniqueName,
                    Process = buildDefinition.Process.GetType().Name,
                    WebLink = (buildDefinition.Links.Links["web"] as ReferenceLink)?.Href,
                    RepositoryName = buildDefinition.Repository?.Name,
                    RepositoryId = buildDefinition.Repository?.Id,
                    Organization = this.vssClient.OrganizationName,
                    GZipCompressedJsonData = Helper.Compress(buildDefinition),
                };
                buildDefinitionEntities.Add(buildDefinitionEntity);

                // Parse build definition steps for designer process
                if (buildDefinition.Process.GetType() == typeof(DesignerProcess))
                {
                    DesignerProcess process = (DesignerProcess)buildDefinition.Process;
                    int stepNumber = 1;

                    foreach (Phase phase in process.Phases)
                    {
                        foreach (BuildDefinitionStep buildDefinitionStep in phase.Steps)
                        {
                            VssBuildDefinitionStepEntity buildDefinitionStepEntity = new VssBuildDefinitionStepEntity
                            {
                                StepNumber = stepNumber,

                                // Build definition reference
                                BuildDefinitionId = buildDefinition.Id,
                                ProjectId = buildDefinition.Project.Id,
                                ProjectName = buildDefinition.Project.Name,

                                // Phase reference
                                PhaseType = this.GetPhaseType(phase),
                                PhaseRefName = phase.RefName,
                                PhaseName = phase.Name,
                                PhaseQueueId = this.GetPhaseQueueId(phase),

                                // Build definition steps
                                DisplayName = buildDefinitionStep.DisplayName,
                                Enabled = buildDefinitionStep.Enabled,
                                TaskDefinitionId = buildDefinitionStep.TaskDefinition.Id,
                                TaskVersionSpec = buildDefinitionStep.TaskDefinition.VersionSpec,
                                Condition = buildDefinitionStep.Condition,

                                // Extra data context
                                Organization = this.vssClient.OrganizationName,
                                //Data = Helper.SerializeObject(phase.Steps),
                            };
                            buildDefinitionStepEntities.Add(buildDefinitionStepEntity);
                            stepNumber++;
                        }
                    }
                }
                // todo: Need to parse yaml file here
            }

            this.logger.LogInformation("Ingesting build definition data...");
            using VssDbContext dbContext = new VssDbContext(logger, this.sqlServerConnectionString);
            using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
            int buildDefinitionEntitiesResult = dbContext.BulkInsertOrUpdate(buildDefinitionEntities);
            int buildDefinitionStepEntitiesResult = dbContext.BulkInsertOrUpdate(buildDefinitionStepEntities);
            transaction.Commit();
            this.logger.LogInformation($"Done ingesting {buildDefinitionEntitiesResult} build definitions, {buildDefinitionStepEntitiesResult} build definition steps");
        }

        // Clean up any stale data since this is a snapshot of data ingestion
        private void Cleanup()
        {
            this.logger.LogInformation($"Deleting up stale data for {this.vssClient.OrganizationName}...");
            using VssDbContext dbContext = new VssDbContext(logger, this.sqlServerConnectionString);
            using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
            int deletedBuildDefinitionResult = dbContext.BulkDelete(dbContext.VssBuildDefinitionEntities.Where(v => v.Organization == this.vssClient.OrganizationName && v.RowUpdatedDate < Helper.UtcNow));
            int deletedBuildDefinitionStepResult = dbContext.BulkDelete(dbContext.VssBuildDefinitionStepEntities.Where(v => v.Organization == this.vssClient.OrganizationName && v.RowUpdatedDate < Helper.UtcNow).ToList());
            transaction.Commit();
            this.logger.LogInformation($"Done deleting {deletedBuildDefinitionResult} build definitions and {deletedBuildDefinitionStepResult} build definition steps");
        }

        private string GetPhaseType(Phase phase)
        {
            if (phase.Target?.Type == PhaseTargetType.Agent)
            {
                return "Agent";
            }
            else if (phase.Target?.Type == PhaseTargetType.Server)
            {
                return "Server";
            }

            return null;
        }

        // You can only get agent pool information when phase type is agent pool, does not work for ServerType target
        private int? GetPhaseQueueId(Phase phase)
        {
            if (phase.Target?.Type == PhaseTargetType.Agent)
            {
                AgentPoolQueueTarget agentPool = (AgentPoolQueueTarget)phase.Target;
                int? queueId = agentPool.Queue?.Id;
                return queueId;
            }

            return null;
        }
    }
}

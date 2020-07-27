﻿using AzureDevOpsDataCollector.Core.Clients;
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

            // Cleanup stale data
            await this.CleanupAsync();
        }

        private async Task IngestData(List<BuildDefinition> buildDefinitions, TeamProjectReference project)
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
                    PoolName = buildDefinition.Queue.Pool.Name,
                    PoolId = buildDefinition.Queue.Pool.Id,
                    IsHosted = buildDefinition.Queue.Pool.IsHosted,
                    QueueName = buildDefinition.Queue.Name,
                    QueueId = buildDefinition.Queue.Id,
                    CreatedDate = buildDefinition.CreatedDate,
                    UniqueName = buildDefinition.AuthoredBy.UniqueName,
                    Process = buildDefinition.Process.GetType().Name,
                    WebLink = (buildDefinition.Links.Links["web"] as ReferenceLink)?.Href,
                    Organization = this.vssClient.OrganizationName,
                    Data = Helper.SerializeObject(buildDefinition),
                };
                buildDefinitionEntities.Add(buildDefinitionEntity);

                // Parse build definition steps for designer process
                if (buildDefinition.Process.GetType() == typeof(DesignerProcess))
                {
                    DesignerProcess process = (DesignerProcess)buildDefinition.Process;
                    foreach (Phase phase in process.Phases)
                    {
                        int stepNumber = 1;

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
                                Data = Helper.SerializeObject(phase.Steps),
                            };
                            buildDefinitionStepEntities.Add(buildDefinitionStepEntity);
                            stepNumber++;
                        }
                    }
                }
                // todo: Need to parse yaml file here
            }

            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkInsertOrUpdateAsync(buildDefinitionEntities);
            await this.dbContext.BulkInsertOrUpdateAsync(buildDefinitionStepEntities);
            await transaction.CommitAsync();
        }

        // Clean up any stale data since this is a snapshot of data ingestion
        private async Task CleanupAsync()
        {
            using IDbContextTransaction transaction = this.dbContext.Database.BeginTransaction();
            await this.dbContext.BulkDeleteAsync(dbContext.VssBuildDefinitionEntities.Where(v => v.Organization == this.vssClient.OrganizationName && v.RowUpdatedDate < Helper.UtcNow).ToList());
            await this.dbContext.BulkDeleteAsync(dbContext.VssBuildDefinitionStepEntities.Where(v => v.Organization == this.vssClient.OrganizationName && v.RowUpdatedDate < Helper.UtcNow).ToList());
            await transaction.CommitAsync();
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

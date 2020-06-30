using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Clients.AzureDevOps;
using AzureDevOpsDataCollector.Core.Collectors;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Function
{
    /// <summary>
    /// This is a sample Durable Functions orchestration job demonstrating how to call into Azure DevOps Data Collector to collect project and repository data
    /// </summary>
    public static class MicrosoftCollectorFunction
    {
        private static VssDbContext dbContext;
        private static VssClient vssClient;
        private const string FunctionStartName = "Microsoft_Start";
        private const string FunctionOrchestrationName = "Microsoft_Orchestrator";
        private const string FunctionRepositoryAcivityName = "Microsoft_Repository_Activity";
        private const string FunctionProjectAcivityName = "Microsoft_Project_Activity";

        [FunctionName(FunctionStartName)]
        public static async Task StartAsync(
            [TimerTrigger("0 0 0 * * *"), Disable()] TimerInfo info,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync(FunctionOrchestrationName, null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        [FunctionName(FunctionOrchestrationName)]
        public static async Task RunOrchestratorAsync([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger logger)
        {
            string organizationName = "microsoft";
            await TryRun(context, FunctionProjectAcivityName, organizationName, logger);
            await TryRun(context, FunctionRepositoryAcivityName, organizationName, logger);
        }

        [FunctionName(FunctionProjectAcivityName)]
        public static async Task RunProjectCollectorAsync([ActivityTrigger] string organizationName, ILogger logger)
        {
            await RunCollectorAsync(organizationName, CollectorType.Project, logger);
        }

        [FunctionName(FunctionRepositoryAcivityName)]
        public static async Task RunRepositoryCollectorAsync([ActivityTrigger] string organizationName, ILogger logger)
        {
            await RunCollectorAsync(organizationName, CollectorType.Repository, logger);
        }

        private static async Task RunCollectorAsync(string organizationName, CollectorType collectorType, ILogger logger)
        {
            // Create Sql database context
            string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            dbContext = new VssDbContext(logger, sqlConnectionString);

            // Create VssClient
            string vssPersonalAccessToken = Environment.GetEnvironmentVariable("VssPersonalAccessToken");
            vssClient = new VssClient(organizationName, vssPersonalAccessToken, VssTokenType.Basic, logger);

            // Run collector
            CollectorBase collector = null;
            if (collectorType == CollectorType.Project)
            {
                collector = new ProjectCollector(vssClient, dbContext);
            }
            else if (collectorType == CollectorType.Repository)
            {
                collector = new RepositoryCollector(vssClient, dbContext, logger);
            }

            await collector.RunAsync();
        }

        private static async Task TryRun(IDurableOrchestrationContext context, string activityName, string organizationName, ILogger logger)
        {
            try
            {
                await context.CallActivityAsync(activityName, organizationName);
            }
            catch(Exception ex)
            {
                logger.LogError($"Failed to execute activity {activityName} with exception {ex}");
            }
        }
    }
}
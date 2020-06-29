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
    public static class MicrosoftCollectorFunction
    {
        [FunctionName("MicrosoftCollectorFunction_Start")]
        public static async Task TimerTriger(
            [TimerTrigger("0 0 0 * * *")] TimerInfo info,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("MicrosoftCollectorFunction_Orchestrator", null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        [FunctionName("MicrosoftCollectorFunction_Orchestrator")]
        public static async Task RunOrchestratorAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync<string>("MicrosoftProjectCollector_Activity", "microsoft");
            await context.CallActivityAsync<string>("MicrosoftRepositoryCollector_Activity", "microsoft");
        }

        [FunctionName("MicrosoftProjectCollector_Activity")]
        public static async Task ProjectCollector([ActivityTrigger] string organizationName, ILogger logger)
        {
            // Create Sql database context
            string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            VssDbContext dbContext = new VssDbContext(logger, sqlConnectionString);

            // Create VssClient
            string vssPersonalAccessToken = Environment.GetEnvironmentVariable("VssPersonalAccessToken");
            VssClient vssClient = new VssClient(organizationName, vssPersonalAccessToken, VssTokenType.Basic, logger);

            // Collect data
            var collector = new ProjectCollector(vssClient, dbContext);
            await collector.RunAsync();
        }

        [FunctionName("MicrosoftRepositoryCollector_Activity")]
        public static async Task RepositoryCollector([ActivityTrigger] string organizationName, ILogger logger)
        {
            // Create Sql database context
            string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            VssDbContext dbContext = new VssDbContext(logger, sqlConnectionString);

            // Create VssClient
            string vssPersonalAccessToken = Environment.GetEnvironmentVariable("VssPersonalAccessToken");
            VssClient vssClient = new VssClient(organizationName, vssPersonalAccessToken, VssTokenType.Basic, logger);

            // Collect data
            var collector = new RepositoryCollector(vssClient, dbContext, logger);
            await collector.RunAsync();
        }

    }
}
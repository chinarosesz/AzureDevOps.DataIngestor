using AzureDevOpsDataCollector.Core.Clients;
using AzureDevOpsDataCollector.Core.Clients.AzureDevOps;
using AzureDevOpsDataCollector.Core.Collectors;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsCollector.Function
{
    /// <summary>
    /// To execute each function locally without having to wait for timer. 
    /// Create a post request to http://localhost:7071/admin/functions/functionName
    /// with body of json value {"input":""}.
    /// For example: POST http://localhost:7071/admin/functions/MicrosoftProject
    /// with body of {"input":""}
    /// </summary>
    public static class RepositoryCollectorFunction
    {
        /// <summary>
        /// Run everyday at 00:00
        /// </summary>
        [FunctionName("MicrosoftRepository")]
        public static async Task MicrosoftRepository([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger logger)
        {
            await RepositoryCollectorFunction.CollectData("microsoft", logger);
        }

        /// <summary>
        /// Run everyday at 00:00
        /// </summary>
        [FunctionName("MsazureRepository")]
        public static async Task MsazureRepository([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger logger)
        {
            await RepositoryCollectorFunction.CollectData("msazure", logger);
        }

        private static async Task CollectData(string organizationName, ILogger logger)
        {
            // Create Sql database context
            string sqlConnectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            VssDbContext dbContext = new VssDbContext(logger, sqlConnectionString);

            // Create VssClient
            string vssPersonalAccessToken = Environment.GetEnvironmentVariable("VssPersonalAccessToken");
            VssClient vssClient = new VssClient(organizationName, vssPersonalAccessToken, VssTokenType.Basic, logger);

            // Collect data
            RepositoryCollector collector = new RepositoryCollector(vssClient, dbContext, logger);
            await collector.RunAsync();
        }
    }
}

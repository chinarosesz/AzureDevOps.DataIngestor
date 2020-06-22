using AzureDevOpsDataCollector.Core;
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
    public static class ProjectCollectorFunction
    {
        /// <summary>
        /// Run everyday at 00:00
        /// </summary>
        [FunctionName("MicrosoftProject")]
        public static async Task MicrosoftProject([TimerTrigger("0 0 0 * * *")] TimerInfo myTimer, ILogger log)
        {
            await ProjectCollectorFunction.CollectProjectData("microsoft", log);
        }

        private static async Task CollectProjectData(string organizationName, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            VssDbContext dbContext = new VssDbContext(log);

            string azureDevOpsPersonalAccessToken = Environment.GetEnvironmentVariable("AzureDevOpsPersonalAccessToken");
            VssClient vssClient = new VssClient(organizationName, azureDevOpsPersonalAccessToken, VssTokenType.Basic, log);

            ProjectCollector projectCollector = new ProjectCollector(vssClient, dbContext);
            await projectCollector.RunAsync();
        }
    }
}

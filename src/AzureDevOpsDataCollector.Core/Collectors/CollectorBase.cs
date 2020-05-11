using AzureDevOpsDataCollector.Core.Clients;
using System;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public abstract class CollectorBase
    {
        protected readonly AzureDevOpsClient azureDevOpsClient;
        protected readonly DateTime Now = DateTime.UtcNow;
        protected readonly AzureDevOpsDbContext dbContext;

        public CollectorBase(AzureDevOpsClient azureDevOpsClient, AzureDevOpsDbContext dbContext)
        {
            this.azureDevOpsClient = azureDevOpsClient;
            this.dbContext = dbContext;
        }

        protected void DisplayProjectHeader(string project)
        {
            Logger.WriteLine();
            Logger.WriteLine($"{this.GetType().Name}:Collecting data for project {project}".ToUpper());
        }
    }
}
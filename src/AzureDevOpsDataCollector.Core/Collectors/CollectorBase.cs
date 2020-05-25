using AzureDevOpsDataCollector.Core.Clients;
using System;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public abstract class CollectorBase
    {
        protected readonly VssClientConnector vssClientConnector;
        protected readonly DateTime Now = DateTime.UtcNow;
        protected readonly AzureDevOpsDbContext dbContext;

        public CollectorBase(VssClientConnector vssClientConnector, AzureDevOpsDbContext dbContext)
        {
            this.vssClientConnector = vssClientConnector;
            this.dbContext = dbContext;
        }

        protected void DisplayProjectHeader(string project)
        {
            Logger.WriteLine();
            Logger.WriteLine($"{this.GetType().Name}:Collecting data for project {project}".ToUpper());
        }
    }
}
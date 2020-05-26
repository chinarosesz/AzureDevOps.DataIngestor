using AzureDevOpsDataCollector.Core.Clients;
using System;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public abstract class CollectorBase
    {
        protected readonly DateTime Now = DateTime.UtcNow;

        protected void DisplayProjectHeader(string project)
        {
            Logger.WriteLine();
            Logger.WriteLine($"{this.GetType().Name}:Collecting data for project {project}".ToUpper());
        }
    }
}
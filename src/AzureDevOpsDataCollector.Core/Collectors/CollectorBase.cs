using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public abstract class CollectorBase
    {
        public abstract Task RunAsync();

        public void DisplayProjectHeader(ILogger logger, string project)
        {
            logger.LogInformation($"Collecting data for project {project}".ToUpper());
        }
    }
}
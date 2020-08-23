using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Ingestors
{
    public abstract class BaseIngestor
    {
        public abstract Task RunAsync();

        public void DisplayProjectHeader(ILogger logger, string project)
        {
            logger.LogInformation($"Collecting data for project {project}".ToUpper());
        }
    }
}
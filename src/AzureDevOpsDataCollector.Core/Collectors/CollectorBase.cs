using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Collectors
{
    public abstract class CollectorBase
    {
        public abstract Task RunAsync();
    }
}
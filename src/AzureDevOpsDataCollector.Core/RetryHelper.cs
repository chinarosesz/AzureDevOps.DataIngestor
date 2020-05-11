using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class RetryHelper
    {
        public static Task<T> WhenAzureDevOpsThrottled<T>(Func<Task<T>> action)
        {
            Task<T> result = Policy
                .Handle<VssServiceException>()
                .WaitAndRetryAsync(3, sleepDurations => TimeSpan.FromMinutes(5))
                .ExecuteAsync(action);

            return result;
        }
    }
}

using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class RetryHelper
    {
        public static Task<T> WhenVssException<T>(Func<Task<T>> action)
        {
            int totalRetryCount = 3;

            Task<T> result = Policy
                .Handle<VssServiceException>()
                .WaitAndRetryAsync(totalRetryCount, sleepDurations => TimeSpan.FromSeconds(5), (result, timeSpan, retryCount, context) =>
                {
                    if (result != null)
                    {
                        Logger.WriteLine($"An exception occured. Retrying {retryCount} out of {totalRetryCount} times and waiting for {timeSpan.TotalSeconds} seconds. {result.Message}");
                    }
                })
                .ExecuteAsync(action);

            return result;
        }

        public static Task<T> SleepAndRetry<T>(int retryCount, TimeSpan timeToSleep, Func<Task<T>> action)
        {
            Task<T> result = Policy
                .Handle<AzureDevOpsRateLimitException>()
                .WaitAndRetryAsync(retryCount, sleepDurations => timeToSleep, (result, timeSpan, currentRetryCount, context) =>
                {
                    if (result != null)
                    {
                        Logger.WriteLine($"An exception occured. Retrying {currentRetryCount} out of {retryCount} times and waiting for {timeSpan.TotalSeconds} seconds. {result.Message}");
                    }
                })
                .ExecuteAsync(action);

            return result;
        }

    }
}

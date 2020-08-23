using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Util
{
    public class RetryHelper
    {
        public static Task<T> WhenVssException<T>(Func<Task<T>> action, ILogger logger)
        {
            int totalRetryCount = 3;

            Task<T> result = Policy
                .Handle<VssServiceException>()
                .WaitAndRetryAsync(totalRetryCount, sleepDurations => TimeSpan.FromSeconds(5), (result, timeSpan, retryCount, context) =>
                {
                    if (result != null)
                    {
                        logger.LogInformation($"An exception occured. Retrying {retryCount} out of {totalRetryCount} times and waiting for {timeSpan.TotalSeconds} seconds. {result.Message}");
                    }
                })
                .ExecuteAsync(action);

            return result;
        }

        public static async Task<T> SleepAndRetry<T>(TimeSpan timeToSleep, ILogger logger, Func<Task<T>> action)
        {
            if (timeToSleep.TotalSeconds > 0)
            {
                logger.LogInformation($"Sleeping for {timeToSleep.TotalSeconds} seconds");
                await Task.Delay(timeToSleep);
            }
            
            return await action();
        }
    }
}

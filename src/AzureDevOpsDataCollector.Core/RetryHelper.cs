using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
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

        public static Task<T> SleepAndRetry<T>(int retryCount, TimeSpan timeToSleep, ILogger logger, Func<Task<T>> action)
        {
            Task<T> result = Policy
                .Handle<AzureDevOpsRateLimitException>()
                .WaitAndRetryAsync(retryCount, sleepDurations => timeToSleep, (result, timeSpan, currentRetryCount, context) =>
                {
                    if (result != null)
                    {
                        logger.LogInformation($"An exception occured. Retrying {currentRetryCount} out of {retryCount} times and waiting for {timeSpan.TotalSeconds} seconds. {result.Message}");
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

        internal static Task SleepAndRetry(object value, Func<Task<List<GitRepository>>> p)
        {
            throw new NotImplementedException();
        }
    }
}

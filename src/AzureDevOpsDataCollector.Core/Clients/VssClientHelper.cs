using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssClientHelper
    {
        public static TimeSpan GetRetryAfter(VssResponseContext vssResponseContext)
        {
            TimeSpan retryAfter = vssResponseContext?.Headers.RetryAfter?.Delta.Value ?? TimeSpan.Zero;
            return retryAfter;
        }
    }
}

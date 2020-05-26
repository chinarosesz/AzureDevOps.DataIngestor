using System;
using System.Net.Http;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssHttpContext
    {
        private readonly HttpResponseMessage responseMessage;
        public TimeSpan RetryAfter { get; }
        public string ResponseContent { get; }
        public Uri RequestUri { get; }

        public VssHttpContext()
        {
            this.RetryAfter = new TimeSpan(0);
        }

        public VssHttpContext(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
            this.RetryAfter = this.responseMessage.Headers.RetryAfter == null ? new TimeSpan(0) : this.responseMessage.Headers.RetryAfter.Delta.Value;
            this.ResponseContent = this.responseMessage.Content.ReadAsStringAsync().Result;
            this.RequestUri = this.responseMessage.RequestMessage.RequestUri;
        }
    }
}

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssHttpContext
    {
        private readonly HttpResponseMessage responseMessage;
        public TimeSpan RetryAfter { get; private set; }
        
        public VssHttpContext()
        {
            this.RetryAfter = new TimeSpan(0);
        }

        public VssHttpContext(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
            this.RetryAfter = this.responseMessage.Headers.RetryAfter == null ? new TimeSpan(0) : this.responseMessage.Headers.RetryAfter.Delta.Value;
        }

        public Task<string> ResponseContent 
        { 
            get { return this.responseMessage.Content.ReadAsStringAsync(); }
        }

        public Uri RequestUri
        {
            get { return this.responseMessage.RequestMessage.RequestUri; }
        }
    }
}

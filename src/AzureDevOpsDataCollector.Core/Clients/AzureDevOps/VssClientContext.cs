using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssClientContext
    {
        private readonly HttpResponseMessage responseMessage;

        public VssClientContext(HttpResponseMessage responseMessage)
        {
            this.responseMessage = responseMessage;
        }

        public TimeSpan RetryAfter 
        { 
            get { return this.responseMessage.Headers.RetryAfter == null ? new TimeSpan(0) : this.responseMessage.Headers.RetryAfter.Delta.Value; }
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

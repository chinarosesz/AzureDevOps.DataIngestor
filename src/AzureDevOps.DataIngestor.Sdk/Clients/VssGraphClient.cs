using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using System;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssGraphClient : GraphHttpClient
    {
        private readonly ILogger logger;

        internal VssGraphClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, ILogger logger) : base(baseUrl, credentials, settings)
        {
            this.logger = logger;
        }

        public async Task<GraphStorageKeyResult> GetStorageKeys(string descriptor)
        {
            GraphStorageKeyResult result = await this.GetStorageKeyAsync(descriptor);

            return result;
        }
    }
}

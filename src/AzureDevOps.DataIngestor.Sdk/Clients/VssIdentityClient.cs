using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using System;
using Microsoft.VisualStudio.Services.Identity.Client;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssIdentityClient : IdentityHttpClient
    {
        private readonly ILogger logger;

        internal VssIdentityClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, ILogger logger) : base(baseUrl, credentials, settings)
        {
            this.logger = logger;
        }
    }
}

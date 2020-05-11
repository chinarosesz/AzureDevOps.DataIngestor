using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureDevOpsClient : IDisposable
    {
        public string OrganizationName { get; private set; }
        public VssConnection VssConnection { get; private set; }

        public AzureDevOpsClient(string organization)
        {
            this.OrganizationName = organization;
        }

        /// <summary>
        /// Init Devops clients with either a personal access token (basic auth) or pass in authentication result with an access token (bearer)
        /// </summary>
        public void ConnectWithBasicToken(string personalAccessToken)
        {
            Uri collectionUri = new Uri($"https://dev.azure.com/{this.OrganizationName}");

            // Connect
            Logger.WriteLine($"Connect to {collectionUri}");
            VssBasicCredential basicCredential = new VssBasicCredential(string.Empty, personalAccessToken);
            VssCredentials vssCredentials = basicCredential;
            this.VssConnection = new VssConnection(collectionUri, vssCredentials);

            // Configure timeout and retry on DevOps HTTP clients
            this.VssConnection.Settings.SendTimeout = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// Init Devops clients with either a personal access token (basic auth) or pass in authentication result with an access token (bearer)
        /// </summary>
        public void ConnectWithBearerToken(string bearerToken)
        {
            Uri collectionUri = new Uri($"https://dev.azure.com/{this.OrganizationName}");
            VssOAuthAccessTokenCredential oAuthCredentials = new VssOAuthAccessTokenCredential(bearerToken);
            VssCredentials vssCredentials = oAuthCredentials;
            this.VssConnection = new VssConnection(collectionUri, vssCredentials);

            // Configure timeout and retry on DevOps HTTP clients
            this.VssConnection.Settings.SendTimeout = TimeSpan.FromMinutes(5);
        }

        public void Dispose()
        {
            this.VssConnection?.Dispose();
        }
    }
}

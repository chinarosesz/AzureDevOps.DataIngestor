using AzureDevOpsDataCollector.Core.Clients.AzureDevOps;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssClient 
    {
        private VssGitClient gitClient;

        public string OrganizationName { get; private set; }

        public VssConnection VssConnection { get; private set; }

        public VssGitClient GitClient
        {
            get
            {
                if (this.gitClient == null)
                {
                    this.gitClient = new VssGitClient(this.VssConnection.Uri, this.VssConnection.Credentials);
                }
                return this.gitClient;
            }
        }

        public VssProjectClient ProjectClient
        {
            get
            {
                VssProjectClient client = new VssProjectClient(this.VssConnection.Uri, this.VssConnection.Credentials);
                return client;
            }
        }

        public VssClient(string organization, string token, VssTokenType tokenType = VssTokenType.Basic)
        {
            this.OrganizationName = organization;

            if (tokenType == VssTokenType.Basic)
            {
                this.ConnectWithBasicToken(token);
            }
            else if (tokenType == VssTokenType.Bearer)
            {
                this.ConnectWithBearerToken(token);
            }
        }

        /// <summary>
        /// Init Devops clients with either a personal access token (basic auth)
        /// </summary>
        private void ConnectWithBasicToken(string personalAccessToken)
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
        /// Init Devops clients with an access token (bearer)
        /// </summary>
        private void ConnectWithBearerToken(string bearerToken)
        {
            Uri collectionUri = new Uri($"https://dev.azure.com/{this.OrganizationName}");
            VssOAuthAccessTokenCredential oAuthCredentials = new VssOAuthAccessTokenCredential(bearerToken);
            VssCredentials vssCredentials = oAuthCredentials;
            this.VssConnection = new VssConnection(collectionUri, vssCredentials);

            // Configure timeout and retry on DevOps HTTP clients
            this.VssConnection.Settings.SendTimeout = TimeSpan.FromMinutes(5);
        }
    }
}

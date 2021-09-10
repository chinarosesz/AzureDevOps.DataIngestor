using AzureDevOps.DataIngestor.Sdk.Util;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssClient 
    {
        private VssGitClient gitClient;
        private VssBuildClient buildClient;
        private VssSecurityClient securityClient;
        private VssGraphClient graphClient;
        private VssIdentityClient identityClient;
        private VssHttpClient httpClient;
        private readonly ILogger logger;

        public string OrganizationName { get; private set; }
        
        public VssConnection VssConnection { get; private set; }

        public VssBuildClient BuildClient
        {
            get
            {
                if (this.buildClient == null)
                {
                    this.buildClient = new VssBuildClient(this.VssConnection.Uri, this.VssConnection.Credentials, this.VssConnection.Settings, logger);
                }
                return this.buildClient;
            }
        }

        public VssGitClient GitClient
        {
            get
            {
                if (this.gitClient == null)
                {
                    this.gitClient = new VssGitClient(this.VssConnection.Uri, this.VssConnection.Credentials, logger);
                }
                return this.gitClient;
            }
        }

        public VssProjectClient ProjectClient
        {
            get
            {
                VssProjectClient client = new VssProjectClient(this.VssConnection.Uri, this.VssConnection.Credentials, this.VssConnection.Settings, this.logger);
                return client;
            }
        }

        public VssSecurityClient SecurityClient
        {
            get
            {
                if (this.securityClient == null)
                {
                    this.securityClient = new VssSecurityClient(this.VssConnection.Uri, this.VssConnection.Credentials, this.VssConnection.Settings, this.logger);
                }
                return this.securityClient;
            }
        }

        public VssIdentityClient IdentityClient
        {
            get
            {
                if (this.identityClient == null)
                {
                    this.identityClient = new VssIdentityClient(this.VssConnection.Uri, this.VssConnection.Credentials, this.VssConnection.Settings, this.logger);
                }
                return this.identityClient;
            }
        }
        public VssGraphClient GraphClient
        {
            get
            {
                if (this.graphClient == null)
                {
                    this.graphClient = new VssGraphClient(this.VssConnection.Uri, this.VssConnection.Credentials, this.VssConnection.Settings, this.logger);
                }
                return this.graphClient;
            }
        }
        public VssClient(string organization, string token, VssTokenType tokenType, ILogger logger)
        {
            this.OrganizationName = organization;
            this.logger = logger;
            Helper.AuthenticationHeader = token;

            if (tokenType == VssTokenType.Basic)
            {
                this.ConnectWithBasicToken(token);
            }
            else if (tokenType == VssTokenType.Bearer)
            {
                this.ConnectWithBearerToken(token);
            }
        }

        public VssHttpClient VSSHttpClient
        {
            get
            {
                if (this.httpClient == null)
                {
                    this.httpClient = new VssHttpClient(this.VssConnection.Uri, Helper.AuthenticationHeader, this.logger);
                }
                return this.httpClient;
            }
        }

        /// <summary>
        /// Init Devops clients with either a personal access token (basic auth)
        /// </summary>
        private void ConnectWithBasicToken(string personalAccessToken)
        {
            Uri collectionUri = new Uri($"https://dev.azure.com/{this.OrganizationName}");

            // Connect
            this.logger.LogInformation($"Connect to {collectionUri} using supplied personal access token");
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
            this.logger.LogInformation($"Connect to {collectionUri} using bearer token");

            VssOAuthAccessTokenCredential oAuthCredentials = new VssOAuthAccessTokenCredential(bearerToken);
            VssCredentials vssCredentials = oAuthCredentials;
            this.VssConnection = new VssConnection(collectionUri, vssCredentials);

            // Configure timeout and retry on DevOps HTTP clients
            this.VssConnection.Settings.SendTimeout = TimeSpan.FromMinutes(5);
        }
    }
}

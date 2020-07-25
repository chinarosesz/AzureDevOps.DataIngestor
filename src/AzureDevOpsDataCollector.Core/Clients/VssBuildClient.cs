using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssBuildClient : BuildHttpClient
    {
        private readonly ILogger logger;

        internal VssBuildClient(Uri baseUrl, VssCredentials credentials, ILogger logger) : base(baseUrl, credentials)
        {
            this.logger = logger;
        }

        public async Task<List<BuildDefinitionReference>> GetBuildDefinitionsAsync(string projectName)
        {
            this.logger.LogInformation($"Retrieving build definitions for project {projectName}");

            List<BuildDefinitionReference> buildDefinitionReferences = await RetryHelper.SleepAndRetry(VssClientHelper.GetRetryAfter(this.LastResponseContext), this.logger, async () =>
            {
                List<BuildDefinitionReference> buildDefinitionReferences = new List<BuildDefinitionReference>();
                IPagedList<BuildDefinitionReference> currentDefinitionReferences;
                do
                {
                    currentDefinitionReferences = await this.GetDefinitionsAsync2(project: projectName);
                    buildDefinitionReferences.AddRange(currentDefinitionReferences);
                }
                while (!currentDefinitionReferences.ContinuationToken.IsNullOrEmpty());

                return buildDefinitionReferences;
            });

            this.logger.LogInformation($"Retrieved {buildDefinitionReferences.Count} build definitions");
            return buildDefinitionReferences;
        }

        public async Task<List<BuildDefinition>> GetFullBuildDefinitionsWithRetryAsync(string projectName)
        {
            this.logger.LogInformation($"Retrieving full build definitions for project {projectName}");

            List<BuildDefinition> buildDefinitions = await RetryHelper.SleepAndRetry(VssClientHelper.GetRetryAfter(this.LastResponseContext), this.logger, async () =>
            {
                List<BuildDefinition> definitions = new List<BuildDefinition>();
                IPagedList<BuildDefinition> currentDefinitions;
                do
                {
                    currentDefinitions = await this.GetFullDefinitionsAsync2(project: projectName);
                    definitions.AddRange(currentDefinitions);
                }
                while (!currentDefinitions.ContinuationToken.IsNullOrEmpty());

                return definitions;
            });

            this.logger.LogInformation($"Retrieved {buildDefinitions.Count} build definitions");
            return buildDefinitions;
        }
    }
}

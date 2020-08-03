using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssBuildClient : BuildHttpClient
    {
        private readonly ILogger logger;

        internal VssBuildClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, ILogger logger) : base(baseUrl, credentials, settings)
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

        public async IAsyncEnumerable<List<BuildDefinition>> GetFullBuildDefinitionsWithRetryAsync(string projectName)
        {
            IPagedList<BuildDefinition> currentDefinitions;
            string continuationToken = null;

            do
            {
                currentDefinitions = await RetryHelper.SleepAndRetry(VssClientHelper.GetRetryAfter(this.LastResponseContext), this.logger, async () =>
                {
                    this.logger.LogInformation($"Retrieving full build definitions for project {projectName}");
                    IPagedList<BuildDefinition> retrievedBuildDefs = await this.GetFullDefinitionsAsync2(project: projectName, top: 1000, queryOrder: DefinitionQueryOrder.LastModifiedAscending, continuationToken: continuationToken);
                    this.logger.LogInformation($"Retrieved {retrievedBuildDefs.Count} build definitions");
                    return retrievedBuildDefs;
                });

                continuationToken = currentDefinitions.ContinuationToken;
                yield return currentDefinitions.ToList();
            }
            while (continuationToken != null);
        }
    }
}

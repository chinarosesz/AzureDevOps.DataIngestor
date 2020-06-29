using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssProjectClient : ProjectHttpClient
    {
        private readonly ILogger logger;

        internal VssProjectClient(Uri baseUrl, VssCredentials credentials, ILogger logger) : base(baseUrl, credentials)
        {
            this.logger = logger;
        }

        public async Task<List<TeamProjectReference>> GetProjectsAsync()
        {
            this.logger.LogInformation("Retrieving projects");

            List<TeamProjectReference> projects = await RetryHelper.SleepAndRetry(VssClientHelper.GetRetryAfter(this.LastResponseContext), this.logger, async () =>
            {
                List<TeamProjectReference> tempProjectsList = new List<TeamProjectReference>();
                string continuationToken = null;

                do
                {
                    IPagedList<TeamProjectReference> currentProjects = await this.GetProjects(continuationToken: continuationToken);
                    continuationToken = currentProjects.ContinuationToken;
                    tempProjectsList.AddRange(currentProjects.ToList());
                }
                while (!continuationToken.IsNullOrEmpty());

                return tempProjectsList;
            });

            this.logger.LogInformation($"Retrieved {projects.Count} projects");

            return projects;
        }

        /// <summary>
        /// If projects list is empty get all projects
        /// </summary>
        public async Task<IEnumerable<TeamProjectReference>> GetProjectNamesAsync(IEnumerable<string> projectNames = null)
        {
            List<TeamProjectReference> projects = new List<TeamProjectReference>();

            if (projectNames.IsNullOrEmpty())
            {
                projects = await this.GetProjectsAsync();
            }
            else
            {
                foreach (TeamProjectReference project in projects)
                {
                    projects.Add(project);
                }
            }

            return projects;
        }
    }
}

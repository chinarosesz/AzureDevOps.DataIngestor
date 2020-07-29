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

        internal VssProjectClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, ILogger logger) : base(baseUrl, credentials, settings)
        {
            this.logger = logger;
        }

        public async Task<List<TeamProjectReference>> GetProjectsAsync(IEnumerable<string> projectNames = null)
        {
            this.logger.LogInformation("Retrieving projects");

            // Retrieve all projects
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

            // Return a list of project references if projectNames list is passed in
            if (projectNames.Count() != 0)
            {
                List<TeamProjectReference> filteredProjects = new List<TeamProjectReference>();
                foreach (TeamProjectReference project in projects)
                {
                    if (projectNames.Contains(project.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        filteredProjects.Add(project);
                    }
                }

                projects = filteredProjects;
            }

            this.logger.LogInformation($"Retrieved {projects.Count} projects");

            return projects;
        }
    }
}

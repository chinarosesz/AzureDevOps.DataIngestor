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
        internal VssProjectClient(Uri baseUrl, VssCredentials credentials) : base(baseUrl, credentials)
        {
        }

        public async Task<List<TeamProjectReference>> GetProjectsAsync()
        {
            Logger.WriteLine("Retrieving projects");

            List<TeamProjectReference> projects = new List<TeamProjectReference>();

            projects = await RetryHelper.SleepAndRetry(VssClientHelper.GetRetryAfter(this.LastResponseContext), async () =>
            {
                string continuationToken = null;

                do
                {
                    IPagedList<TeamProjectReference> currentProjects = await this.GetProjects(continuationToken: continuationToken);
                    continuationToken = currentProjects.ContinuationToken;
                    projects.AddRange(currentProjects.ToList());
                }
                while (!continuationToken.IsNullOrEmpty());

                return projects;
            });

            Logger.WriteLine($"Retrieved {projects.Count} projects");

            return projects;
        }

        public async Task<IEnumerable<TeamProjectReference>> GetProjectNamesAsync(IEnumerable<string> projectNames)
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

using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssProjectClient : ProjectHttpClient
    {
        public VssHttpContext HttpContext { get; private set; }

        internal VssProjectClient(Uri baseUrl, VssCredentials credentials) : base(baseUrl, credentials)
        {
        }

        public async Task<List<TeamProjectReference>> GetProjectsAsync()
        {
            List<TeamProjectReference> projects = new List<TeamProjectReference>();

            projects = await RetryHelper.WhenVssException(async () =>
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

        protected override Task<T> ReadJsonContentAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            this.HttpContext = new VssHttpContext(response);
            return base.ReadJsonContentAsync<T>(response, cancellationToken);
        }
    }
}

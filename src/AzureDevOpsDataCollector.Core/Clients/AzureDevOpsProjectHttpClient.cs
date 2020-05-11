using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureDevOpsProjectHttpClient : ProjectHttpClient
    {
        public AzureDevOpsProjectHttpClient(Uri baseUrl, HttpMessageHandler pipeline, bool disposeHandler) : base(baseUrl, pipeline, disposeHandler)
        {
        }

        public async Task<List<TeamProjectReference>> GetProjectsWithRetryAsync()
        {
            List<TeamProjectReference> projects = new List<TeamProjectReference>();

            projects = await RetryHelper.WhenAzureDevOpsThrottled(async () =>
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

        public async Task<IEnumerable<string>> GetProjectNamesWithRetryAsync()
        {
            List<TeamProjectReference> projects = await this.GetProjectsWithRetryAsync();
            IEnumerable<string> projectNames = projects.Select(v => v.Name);
            return projectNames;
        }
    }
}

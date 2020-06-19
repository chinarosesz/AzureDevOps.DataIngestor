using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class VssClientHelper
    {
        public static TimeSpan GetRetryAfter(VssResponseContext vssResponseContext)
        {
            TimeSpan retryAfter = vssResponseContext?.Headers.RetryAfter?.Delta.Value ?? TimeSpan.Zero;
            return retryAfter;
        }

        /// <summary>
        /// Cuurrently suports only Microsoft tenant, user has to be connected on domain
        /// </summary>
        public static async Task<string> GetAzureDevOpsBearerTokenForCurrentUserAsync()
        {
            string tenant = "microsoft.com";

            string aadAuthority = $"https://login.windows.net/{tenant}";

            // Fixed static resource Guid for Azure Devops
            string aadResource = "499b84ac-1321-427f-aa17-267ca6975798";

            // MSA client ID if you don't have an application ID regsitered with Azure
            string aadClientId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1";

            // Login now
            AuthenticationContext authCtx = new AuthenticationContext(aadAuthority);
            string aadUser = $"{Environment.UserName}@{tenant}";
            UserCredential userCredential = new UserCredential(aadUser);
            AuthenticationResult authContext = await authCtx.AcquireTokenAsync(aadResource, aadClientId, userCredential);
            return authContext.AccessToken;
        }
    }
}

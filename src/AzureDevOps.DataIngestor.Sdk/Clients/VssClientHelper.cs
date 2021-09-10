using AzureDevOps.InteractiveLogin;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssClientHelper
    {

        // The Client ID is used by the application to uniquely identify itself to Azure AD
        // Replace with your own if you already have one
        private const string clientId = "872cd9fa-d31f-45e0-9eab-6e460a02d1f1";

        // The Authority is the sign-in URL of the tenant
        private const string authority = "https://login.microsoftonline.com/microsoft.com/v2.0";

        // Constant value to target Azure DevOps. Do not change  
        private static readonly string[] scopes = new string[] { "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation" };

        // MSAL Public client app
        private static IPublicClientApplication application;

        public static TimeSpan GetRetryAfter(VssResponseContext vssResponseContext)
        {
            TimeSpan retryAfter = vssResponseContext?.Headers.RetryAfter?.Delta.Value ?? TimeSpan.Zero;
            return retryAfter;
        }

        /// <summary>
        /// Sign-in user using MSAL and obtain an access token for Azure DevOps
        /// </summary>
        public static async Task<string> SignInUserAndGetTokenUsingMSAL()
        {
            // Initialize the MSAL library by building a public client application
            application = PublicClientApplicationBuilder.Create(clientId).WithAuthority(authority).WithDefaultRedirectUri().Build();
            TokenCacheHelper.EnableSerialization(application.UserTokenCache);

            AuthenticationResult result;

            IEnumerable<IAccount> accounts = await application.GetAccountsAsync();
            // clear the cache. This code will clear cache every time. not sure we need it.
            //while (accounts.Any())
            //{
            //    await application.RemoveAsync(accounts.First());
            //    accounts = (await application.GetAccountsAsync()).ToList();
            //}
            try
            {
                result = await application.AcquireTokenSilent(scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // If the token has expired, prompt the user with a login prompt
                result = await application.AcquireTokenInteractive(scopes)
                        .WithClaims(ex.Claims)
                        .ExecuteAsync();
            }

            return result.AccessToken;
        }
    }
}

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    using AzureDevOps.DataIngestor.Sdk.Util;
    using System;
    using System.Net.Http;

    /// <summary>
    /// Singleton for HTTPClient.
    /// </summary>
    public class HttpClientSingleton
    {
        private static HttpClient client;

        /// <summary>
        /// Gets the HttpClient - if null, creates it with the appropriate settings.
        /// </summary>
        /// <returns>HttpClient.</returns>
        public static HttpClient Instance
        {
            // TODO:  See if .NET CORE 3.1 has the same problem with HttpClient as described here:
            // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/.
            get
            {
                try
                {
                    if (client == null)
                    {
                        if (Helper.AuthenticationHeader != string.Empty)
                        {
                            client = new HttpClient();
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(
                                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.DefaultRequestHeaders.Add("User-Agent", "ManagedClientConsoleAppSample");
                            client.DefaultRequestHeaders.Add("X-TFS-FedAuthRedirect", "Suppress");
                            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Helper.AuthenticationHeader}");
                            //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                            //    "Bearer",
                            //        Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(
                            //            string.Format($"{string.Empty}:{Helper.AuthenticationHeader}}"))));
                        }
                        else
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is UnauthorizedAccessException)
                    {
                        Console.WriteLine($"Please check your authorization token");
                        throw ex;
                    }

                    Console.WriteLine($"Exception caught: {ex.ToString()}");
                    throw ex;
                }

                return client;
            }
        }
    }
}

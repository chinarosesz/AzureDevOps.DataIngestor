using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureDevOps.DataIngestor.Sdk.Clients
{
    public class VssHttpClient : HttpClient
    {
        private readonly ILogger logger;

        internal VssHttpClient(Uri baseUrl, string token, ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// HttpGetAsync - async GET command.
        /// </summary>
        /// <typeparam name="T">Returns generic type.</typeparam>
        /// <param name="url">URL to query</param>
        /// <param name="useJsonOptions">JSON options, typically around camel case to pascal case or vice versa.</param>
        /// <returns>Generic type T.</returns>
        public async Task<T> HttpGetAsync<T>(string url, bool useJsonOptions = true)
        {
            try
            {
                using (HttpResponseMessage response = await HttpClientSingleton.Instance.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode && !response.StatusCode.Equals(203))
                    {
                        string returnedResponse = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = new JsonPascalNamingPolicy(),
                        };
                        if (useJsonOptions == false)
                        {
                            T returnedItemWithoutOptions = JsonSerializer.Deserialize<T>(returnedResponse);
                            return returnedItemWithoutOptions;
                        }

                        T returnedItem = JsonSerializer.Deserialize<T>(returnedResponse, options);
                        return returnedItem;
                    }
                    else
                    {
                        // TODO: Handle this better in case we don't get a successfull response and does not throw
                        logger.LogError($"Check URL {url} and {response}");
                        throw new HttpRequestException("Request returned non-Successfull code");
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Handle this better
                logger.LogError($"General Exception caught - {ex.ToString()}");
                throw;
            }
        }

        /// <summary>
        /// Converts PascalCase to camelcase (needed for Json formatting).
        /// </summary>
        internal class JsonPascalNamingPolicy : JsonNamingPolicy
        {
            /// <summary>
            /// Actual method.  Takes a Pascalcase string and converts to camelcase.
            /// </summary>
            /// <param name="name">String in question.</param>
            /// <returns>Camelcase string.</returns>
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return name;
                }

                var newName = string.Concat(name[0].ToString().ToLower(), name.Substring(1));
                return newName;
            }
        }
    }
}

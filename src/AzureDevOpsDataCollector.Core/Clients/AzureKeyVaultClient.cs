using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Threading.Tasks;

namespace AzureDevOpsDataCollector.Core.Clients
{
    public class AzureKeyVaultClient
    {
        private string peronsalAccessToken = null;

        public async Task<string> GetPersonalAccessTokenAsync()
        {
            Logger.WriteLine("Connect to KeyVault to get Personal Access Token");

            if (peronsalAccessToken != null)
            {
                return peronsalAccessToken;
            }

            peronsalAccessToken = Environment.GetEnvironmentVariable("PersonalAccessToken");
            if (peronsalAccessToken == null)
            {
                SecretBundle secretBundle = await GetSecret("<insert url to keyvault here>");
                peronsalAccessToken = secretBundle.Value;
            }
            return peronsalAccessToken;
        }

        private static async Task<SecretBundle> GetSecret(string secretUrl)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

            using (KeyVaultClient keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback)))
            {
                SecretBundle secretBundle = await keyVaultClient.GetSecretAsync(secretUrl);
                return secretBundle;
            }
        }
    }
}
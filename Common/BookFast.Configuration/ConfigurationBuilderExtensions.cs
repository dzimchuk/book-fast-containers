using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BookFast.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder)
        {
            var builtConfig = builder.Build();
            var keyVaultName = builtConfig["KeyVaultName"];

            if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                var userAssignedClientId = builtConfig["UserAssignedClientId"];

                var credentials = string.IsNullOrWhiteSpace(userAssignedClientId)
                    ? new DefaultAzureCredential()
                    : new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

                builder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net/"), credentials);
            }
            else
            {
                // docker-compose local mode
                var mountedVolume = builtConfig["KeyVault:Path"];
                if (!string.IsNullOrWhiteSpace(mountedVolume))
                {
                    keyVaultName = File.ReadAllText($"{mountedVolume}/keyVaultName");
                    var tenantId = File.ReadAllText($"{mountedVolume}/tenantId");
                    var clientId = File.ReadAllText($"{mountedVolume}/clientId");
                    var secret = File.ReadAllText($"{mountedVolume}/clientSecret");

                    var credentials = new ClientSecretCredential(tenantId, clientId, secret);
                    builder.AddAzureKeyVault(new Uri($"https://{keyVaultName}.vault.azure.net/"), credentials);
                }
            }

            return builder;
        }
    }
}

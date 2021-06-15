using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;

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

            return builder;
        }
    }
}

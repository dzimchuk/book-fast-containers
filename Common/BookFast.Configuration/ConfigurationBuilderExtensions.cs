using Microsoft.Extensions.Configuration;
using System.IO;

namespace BookFast.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder)
        {
            var builtConfig = builder.Build();
            var keyVaultName = File.ReadAllText($"{builtConfig["KeyVault:Path"]}/keyVaultName");
            var clientId = File.ReadAllText($"{builtConfig["KeyVault:Path"]}/clientId");
            var secret = File.ReadAllText($"{builtConfig["KeyVault:Path"]}/clientSecret");

            builder.AddAzureKeyVault(
                    $"https://{keyVaultName}.vault.azure.net/",
                    clientId,
                    secret);

            return builder;
        }
    }
}

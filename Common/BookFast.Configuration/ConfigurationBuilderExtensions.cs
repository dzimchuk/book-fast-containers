using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BookFast.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAzureKeyVault(this IConfigurationBuilder builder)
        {
            var builtConfig = builder.Build();

            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, builtConfig["KeyVault:AzureADCertThumbprint"], false);

                builder.AddAzureKeyVault(
                    $"https://{builtConfig["KeyVault:KeyVaultName"]}.vault.azure.net/",
                    builtConfig["KeyVault:AzureADApplicationId"],
                    certs.OfType<X509Certificate2>().Single());

                store.Close();
            }

            return builder;
        }
    }
}

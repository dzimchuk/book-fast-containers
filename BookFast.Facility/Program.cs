using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace BookFast.Facility
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(options => options.ValidateScopes = false) // scoped services (e.g. DbContext) cannot be used in singletons (e.g. IHostedService)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        var builtConfig = config.Build();

                        using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                        {
                            store.Open(OpenFlags.ReadOnly);
                            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, builtConfig["KeyVault:AzureADCertThumbprint"], false);

                            config.AddAzureKeyVault(
                                $"https://{builtConfig["KeyVault:KeyVaultName"]}.vault.azure.net/",
                                builtConfig["KeyVault:AzureADApplicationId"],
                                certs.OfType<X509Certificate2>().Single());

                            store.Close();
                        }
                    }
                })
                .UseStartup<Startup>();
    }
}

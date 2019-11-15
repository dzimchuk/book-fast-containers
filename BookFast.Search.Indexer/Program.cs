using System.Collections.Generic;
using BookFast.Configuration;
using BookFast.SeedWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.Search.Indexer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        config.AddAzureKeyVault();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService(hostContext.Configuration);

                    var modules = new List<ICompositionModule>
                          {
                              new Composition.CompositionModule(),
                              new Adapter.Composition.CompositionModule(),
                              new Facility.Client.Composition.CompositionModule()
                          };

                    foreach (var module in modules)
                    {
                        module.AddServices(services, hostContext.Configuration);
                    }
                });
    }
}

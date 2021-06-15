using BookFast.Configuration;
using BookFast.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BookFast.Facility
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider(options => options.ValidateScopes = true) // scoped services (e.g. DbContext) cannot be used in singletons (e.g. IHostedService)
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsStaging() || context.HostingEnvironment.IsProduction())
                    {
                        config.AddAzureKeyVault();
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseCustomServiceProviderFactory();
    }
}

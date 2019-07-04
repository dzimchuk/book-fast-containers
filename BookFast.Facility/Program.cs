using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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
                .UseStartup<Startup>();
    }
}

using System.Collections.Generic;
using BookFast.SeedWork;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.Search
{
    public class Startup
    {
        private const string apiTitle = "Book Fast Search API";
        private const string apiVersion = "v1";

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAndConfigureMvc();

            services.AddApplicationInsightsTelemetry(configuration);

            services.AddSwashbuckle(configuration, apiTitle, apiVersion, "BookFast.Search.xml");

            var modules = new List<ICompositionModule>
                          {
                              new Adapter.Composition.CompositionModule()
                          };

            foreach (var module in modules)
            {
                module.AddServices(services, configuration);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(apiTitle, apiVersion);
        }
    }
}

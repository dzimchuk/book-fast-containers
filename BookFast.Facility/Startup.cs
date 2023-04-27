using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookFast.Facility.Core;
using BookFast.Facility.Infrastructure;
using BookFast.Api;

namespace BookFast.Facility
{
    public class Startup
    {
        private const string apiTitle = "Book Fast Facility API";
        private const string apiVersion = "v1";

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(configuration);
            services.AddAuthorizationPolicies();

            services.AddSecurityContext();
            services.AddAndConfigureControllers();

            services.AddApplicationInsightsTelemetry(configuration);

            services.AddApplicationServices();
            services.AddDbContext(configuration);

            //services.AddIntegrationEventPublisher(configuration);
            //services.AddIntegrationEventReceiver(configuration, new IntegrationEventMapper());

            services.AddSwashbuckle(configuration, apiTitle, apiVersion, "BookFast.Facility.xml");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSecurityContext();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(apiTitle, apiVersion);
        }
    }
}

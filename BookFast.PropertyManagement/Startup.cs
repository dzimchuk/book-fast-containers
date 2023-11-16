using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BookFast.Api;
using BookFast.Api.SecurityContext;
using BookFast.Api.Swagger;
using BookFast.PropertyManagement.Core;
using BookFast.PropertyManagement.Infrastructure;

namespace BookFast.PropertyManagement
{
    public class Startup
    {
        private const string apiTitle = "Book Fast Property Management API";
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

            //services.AddSwashbuckle(configuration, apiTitle, apiVersion, "BookFast.PropertyManagement.xml");
            services.AddSwaggerServices(configuration, xmlDocFileName: "BookFast.PropertyManagement.xml");
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

            app.UseSwagger(configuration);
        }
    }
}

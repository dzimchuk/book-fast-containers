using System.Collections.Generic;
using BookFast.Api.Authentication;
using BookFast.Facility.Integration;
using BookFast.ReliableEvents;
using BookFast.Security;
using BookFast.SeedWork;
using BookFast.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            AddAuthentication(services, configuration);

            services.AddSecurityContext();
            services.AddAndConfigureMvc();

            services.AddApplicationInsightsTelemetry(configuration);

            services.AddCommandContext();
            services.AddReliableEventsDispatcher(configuration, new DefaultReliableEventMapper(typeof(Domain.Events.FacilityCreatedEvent).Assembly));

            services.AddIntegrationEventPublisher(configuration);
            services.AddIntegrationEventReceiver(configuration, new IntegrationEventMapper());

            RegisterAuthorizationPolicies(services);

            services.AddSwashbuckle(configuration, apiTitle, apiVersion, "BookFast.Facility.xml");

            var modules = new List<ICompositionModule>
                          {
                              new CommandStack.Composition.CompositionModule(),
                              new Data.Composition.CompositionModule()
                          };

            foreach (var module in modules)
            {
                module.AddServices(services, configuration);
            }
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseSecurityContext();
            app.UseMvc();

            app.UseSwagger(apiTitle, apiVersion);
        }

        private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var authOptions = configuration.GetSection("Authentication:AzureAd").Get<AuthenticationOptions>();

            services.AddAuthentication(Constants.OrganizationalAuthenticationScheme)
                .AddJwtBearer(Constants.OrganizationalAuthenticationScheme, options =>
                {
                    options.Authority = authOptions.Authority;
                    options.Audience = authOptions.Audience;

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidIssuers = authOptions.ValidIssuersAsArray
                    };
                });
        }

        private static void RegisterAuthorizationPolicies(IServiceCollection services)
        {
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("Facility.Write", config =>
                    {
                        config.RequireRole(InteractorRole.FacilityProvider.ToString(), InteractorRole.ImporterProcess.ToString());
                    });
                });
        }
    }
}

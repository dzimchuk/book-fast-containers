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
using Microsoft.Extensions.Options;

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
            services.Configure<AuthenticationOptions>(configuration.GetSection("Authentication:AzureAd"));
            var serviceProvider = services.BuildServiceProvider();
            var authOptions = serviceProvider.GetService<IOptions<AuthenticationOptions>>();

            services.AddAuthentication(Constants.OrganizationalAuthenticationScheme)
                .AddJwtBearer(Constants.OrganizationalAuthenticationScheme, options =>
                {
                    options.Authority = authOptions.Value.Authority;
                    options.Audience = authOptions.Value.Audience;

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidIssuers = authOptions.Value.ValidIssuersAsArray
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

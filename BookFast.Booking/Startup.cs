using System.Collections.Generic;
using BookFast.Booking.Integration;
using BookFast.ReliableEvents;
using BookFast.SeedWork;
using BookFast.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking
{
    public class Startup
    {
        private const string apiTitle = "Book Fast Booking API";
        private const string apiVersion = "v1";

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddB2CAuthentication(configuration);

            services.AddSecurityContext();
            services.AddAndConfigureMvc();

            services.AddApplicationInsightsTelemetry(configuration);

            services.AddCommandContext();
            services.AddReliableEventsDispatcher(configuration["ServiceBus:Booking:NotificationQueueName"],
                                                 configuration["ServiceBus:Booking:NotificationQueueConnection"],
                                                 new DefaultReliableEventMapper(typeof(Domain.Events.BookingCreatedEvent).Assembly));

            services.AddIntegrationEventPublisher(configuration);
            services.AddIntegrationEventReceiver(configuration, new IntegrationEventMapper());

            services.AddSwashbuckle(configuration, apiTitle, apiVersion, "BookFast.Booking.xml");

            var modules = new List<ICompositionModule>
                          {
                              new CommandStack.Composition.CompositionModule(),
                              new Data.Composition.CompositionModule()
                          };

            foreach (var module in modules)
            {
                module.AddServices(services, configuration);
            }

            services.Configure<TestOptions>(configuration.GetSection("Test"));
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
    }
}

using BookFast.Infrastructure;
using BookFast.Infrastructure.Filters;
using BookFast.Integration;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMassTransit<TDbContext>(this IServiceCollection services,
            IConfiguration configuration, IHostEnvironment env,
            Action<IBusRegistrationConfigurator, ServiceBusOptions> configureConsumers = null)
            where TDbContext : DbContext
        {
            var serviceBusOptions = configuration.GetServiceBusOptions(env);

            if (string.IsNullOrWhiteSpace(serviceBusOptions.ConnectionString)) // unit tests
            {
                return;
            }

            services.AddMailNotificationQueue(serviceBusOptions);

            services.AddMassTransit(config =>
            {
                configureConsumers?.Invoke(config, serviceBusOptions);

                config.AddEntityFrameworkOutbox<TDbContext>(outboxOptions =>
                {
                    outboxOptions.QueryDelay = TimeSpan.FromMinutes(1);

                    outboxOptions.UseSqlServer();
                    outboxOptions.UseBusOutbox(cfg =>
                    {
                        //cfg.DisableDeliveryService();
                    });
                });

                config.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(serviceBusOptions.ConnectionString);

                    cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                    cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}

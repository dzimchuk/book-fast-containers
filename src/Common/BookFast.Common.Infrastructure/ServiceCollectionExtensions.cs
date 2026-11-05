using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Infrastructure;
using BookFast.Common.Infrastructure.Clock;
using BookFast.Common.Infrastructure.Filters;
using BookFast.Common.Infrastructure.Integration;
using BookFast.Common.Infrastructure.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookFast.Common.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommonInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        }

        public static IServiceCollection AddDatabaseReadinessCheck<TContext>(this IServiceCollection services, string name)
            where TContext : DbContext
        {
            services.AddHealthChecks().AddDbContextCheck<TContext>(name, tags: ["ready"]);
            return services;
        }

        public static void AddMassTransit(this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator> configureServiceSpecifics)
        {
            services.TryAddTransient<IMailNotificationQueue, MailNotificationQueue>();
            services.TryAddTransient<IIntegrationEventPublisher, IntegrationEventPublisher>();
            services.TryAddTransient<ILocalBrokeredMessageSender, LocalBrokeredMessageSender>();

            var connectionString = configuration.GetConnectionString("MessageBus");
            var transport = configuration["Messaging:Transport"];

            services.AddMassTransit(config =>
            {
                configureServiceSpecifics?.Invoke(config);

                if (string.Equals(transport, MessagingTransports.AzureServiceBus, StringComparison.OrdinalIgnoreCase))
                {
                    config.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(connectionString);

                        cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                        cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                        cfg.ConfigureEndpoints(context);
                    });
                }
                else
                {
                    config.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(new Uri(connectionString), hostConfig =>
                        {
                            //hostConfig.Username("");
                            //hostConfig.Password("");
                        });

                        cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                        cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                        cfg.ConfigureEndpoints(context);
                    });
                }
            });
        }
    }
}

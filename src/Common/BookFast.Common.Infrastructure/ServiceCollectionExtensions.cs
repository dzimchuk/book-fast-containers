using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Integration;
using BookFast.Common.Infrastructure;
using BookFast.Common.Infrastructure.Clock;
using BookFast.Common.Infrastructure.Filters;
using BookFast.Common.Infrastructure.Integration;
using MassTransit;
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

        public static void AddMassTransit(this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator> configureServiceSpecifics)
        {
            services.TryAddTransient<IMailNotificationQueue, MailNotificationQueue>();
            services.TryAddTransient<IIntegrationEventPublisher, IntegrationEventPublisher>();

            services.AddMassTransit(config =>
            {
                configureServiceSpecifics?.Invoke(config);

                //config.UsingInMemory((context, cfg) =>
                //{
                //    cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                //    cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                //    cfg.ConfigureEndpoints(context);
                //});

                config.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(configuration.GetConnectionString("MessageBus")), hostConfig =>
                    {
                        //hostConfig.Username("");
                        //hostConfig.Password("");
                    });

                    cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                    cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                    cfg.ConfigureEndpoints(context);
                });
            });
        }
    }
}

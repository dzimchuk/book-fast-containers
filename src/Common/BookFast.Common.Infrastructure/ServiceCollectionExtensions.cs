using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Integration;
using BookFast.Common.Infrastructure;
using BookFast.Common.Infrastructure.Clock;
using BookFast.Common.Infrastructure.Filters;
using BookFast.Common.Infrastructure.Integration;
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

        public static void AddMassTransit<TDbContext>(this IServiceCollection services,
            EventBusOptions eventBusOptions,
            Action<IBusRegistrationConfigurator, EventBusOptions> configureConsumers)
            where TDbContext : DbContext
        {
            services.AddMailNotificationQueue(eventBusOptions);

            services.AddMassTransit(config =>
            {
                configureConsumers?.Invoke(config, eventBusOptions);

                config.AddEntityFrameworkOutbox<TDbContext>(outboxOptions =>
                {
                    outboxOptions.QueryDelay = TimeSpan.FromMinutes(1);

                    outboxOptions.UsePostgres();
                    outboxOptions.UseBusOutbox(cfg =>
                    {
                        //cfg.DisableDeliveryService();
                    });
                });

                config.UsingInMemory((context, cfg) =>
                {
                    cfg.UseSendFilter(typeof(MessageSizeFilter<>), context);
                    cfg.UsePublishFilter(typeof(MessageSizeFilter<>), context);

                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        private static IServiceCollection AddMailNotificationQueue(this IServiceCollection services, EventBusOptions eventBusOptions)
        {
            EndpointConvention.Map<MailMessage<object>>(new Uri($"queue:{eventBusOptions.GetMailSenderQueueName()}"));

            return services.AddTransient<IMailNotificationQueue, MailNotificationQueue>();
        }

    }
}

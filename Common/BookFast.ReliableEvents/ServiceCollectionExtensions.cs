using BookFast.ReliableEvents.CommandStack;
using BookFast.SeedWork.Modeling;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.ReliableEvents
{
    public static class ServiceCollectionExtensions
    {
        public static void AddReliableEventsDispatcher(this IServiceCollection services,
                                                       string notificationQueueName,
                                                       string notificationQueueConnection,
                                                       IReliableEventMapper reliableEventMapper)
        {
            services.AddSingleton<ReliableEventsDispatcher>();
            services.AddSingleton<IHostedService, ReliableEventsDispatcherService>();

            services.Configure<ConnectionOptions>(options =>
            {
                options.NotificationQueueName = notificationQueueName;
                options.NotificationQueueConnection = notificationQueueConnection;
            });

            services.AddSingleton<INotificationHandler<EventsAvailableNotification>, NotificationPublisher>();

            services.AddSingleton(reliableEventMapper);

            services.AddScoped<IIntegrationEventPublisher, DummyIntegrationEventPublisher>();
        }

        public static void AddCommandContext(this IServiceCollection services)
        {
            services.AddScoped<CommandContext>();
        }
    }
}

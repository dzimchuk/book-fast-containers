using BookFast.ReliableEvents.CommandStack;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookFast.ReliableEvents
{
    public static class ServiceCollectionExtensions
    {
        public static void AddReliableEventsDispatcher(this IServiceCollection services,
                                                       string notificationQueueName,
                                                       string notificationQueueConnection,
                                                       IReliableEventMapper reliableEventMapper)
        {
            services.AddSingleton(serviceProvider =>
                new ReliableEventsDispatcher(serviceProvider.GetRequiredService<ILogger<ReliableEventsDispatcher>>(),
                                             serviceProvider,
                                             notificationQueueName,
                                             notificationQueueConnection,
                                             reliableEventMapper));
            services.AddSingleton<IHostedService, ReliableEventsDispatcherService>();

            services.AddSingleton<INotificationHandler<EventsAvailableNotification>, NotificationPublisher>();
        }

        public static void AddCommandContext(this IServiceCollection services)
        {
            services.AddScoped<CommandContext>();
        }
    }
}

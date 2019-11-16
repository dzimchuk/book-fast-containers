using BookFast.SeedWork.Modeling;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.ServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static void AddIntegrationEventPublisher(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionOptions>(configuration.GetSection("ServiceBus"));
            services.AddSingleton<TopicClientProvider>();
            services.AddScoped<INotificationHandler<IntegrationEvent>, IntegrationEventPublisher>(); // won't work unless DI container support covariant handlers
            services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>(); // current workaround
        }

        public static void AddIntegrationEventReceiver(this IServiceCollection services, IConfiguration configuration, IEventMapper eventMapper)
        {
            services.Configure<ConnectionOptions>(configuration.GetSection("ServiceBus"));
            services.AddHostedService<IntegrationEventReceiver>();
            services.AddSingleton(eventMapper);
        }
    }
}

using BookFast.Integration.Internal;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Integration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMailNotificationQueue(this IServiceCollection services, ServiceBusOptions serviceBusOptions)
        {
            EndpointConvention.Map<MailMessage<object>>(new Uri($"queue:{serviceBusOptions.GetMailSenderQueueName()}"));

            return services.AddTransient<IMailNotificationQueue, MailNotificationQueue>();
        }
    }
}

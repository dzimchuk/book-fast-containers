using BookFast.Security;
using BookFast.SeedWork.Modeling;
using MediatR;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BookFast.ServiceBus
{
    internal class IntegrationEventPublisher : INotificationHandler<IntegrationEvent>, IIntegrationEventPublisher
    {
        private readonly TopicClientProvider topicClientProvider;
        private readonly ISecurityContext securityContext;

        public IntegrationEventPublisher(TopicClientProvider topicClientProvider, ISecurityContext securityContext)
        {
            this.topicClientProvider = topicClientProvider;
            this.securityContext = securityContext;
        }

        public Task Handle(IntegrationEvent notification, CancellationToken cancellationToken)
        {
            var topicClient = topicClientProvider.Client;

            if (topicClient == null)
            {
                return Task.CompletedTask;
            }

            var jsonMessage = JsonConvert.SerializeObject(notification);

            var message = new Message
            {
                MessageId = notification.EventId.ToString(),
                Body = Encoding.UTF8.GetBytes(jsonMessage),
                Label = notification.GetType().Name
            };

            AddSecurityContext(message);

            return topicClient.SendAsync(message);
        }

        public Task PublishAsync(IntegrationEvent @event)
        {
            return Handle(@event, CancellationToken.None);
        }

        private void AddSecurityContext(Message message)
        {
            message.UserProperties.Add(Constants.TenantId, securityContext.GetCurrentTenant());
            message.UserProperties.Add(Constants.ObjectId, securityContext.GetCurrentUser());
        }
    }
}

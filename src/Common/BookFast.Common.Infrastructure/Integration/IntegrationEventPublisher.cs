using BookFast.Common.Application.Integration;
using MassTransit;

namespace BookFast.Common.Infrastructure.Integration
{
    internal class IntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        {
            return publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}

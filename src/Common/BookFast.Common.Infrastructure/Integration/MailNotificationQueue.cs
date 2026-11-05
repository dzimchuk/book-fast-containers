using BookFast.Common.Application.Integration;
using MassTransit;

namespace BookFast.Common.Infrastructure.Integration
{
    internal class MailNotificationQueue : IMailNotificationQueue
    {
        private readonly ISendEndpointProvider sendEndpointProvider;

        public MailNotificationQueue(ISendEndpointProvider sendEndpointProvider)
        {
            this.sendEndpointProvider = sendEndpointProvider;
        }

        public async Task EnqueueMessageAsync<TModel>(MailMessage<TModel> message, CancellationToken cancellationToken = default)
        {
            if (EndpointConvention.TryGetDestinationAddress<IMailMessage>(out var destinationAddress))
            {
                var endpoint = await sendEndpointProvider.GetSendEndpoint(destinationAddress);
                await endpoint.Send(message, cancellationToken);
            }
        }
    }
}

using MassTransit;

namespace BookFast.Integration.Internal
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
            if (EndpointConvention.TryGetDestinationAddress<MailMessage<object>>(out var destinationAddress))
            {
                var endpoint = await sendEndpointProvider.GetSendEndpoint(destinationAddress);
                await endpoint.Send(message, cancellationToken);
            }
        }
    }
}

using BookFast.Common.Application.Messaging;
using MassTransit;

namespace BookFast.Common.Infrastructure.Messaging
{
    internal class LocalBrokeredMessageSender(ISendEndpointProvider sendEndpointProvider) : ILocalBrokeredMessageSender
    {
        public Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) =>
            sendEndpointProvider.Send(message, cancellationToken);
    }
}

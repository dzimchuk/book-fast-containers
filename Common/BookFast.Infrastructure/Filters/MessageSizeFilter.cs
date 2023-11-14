using MassTransit;
using System.Text.Json;

namespace BookFast.Infrastructure.Filters
{
    internal class MessageSizeFilter<TMessage> : IFilter<PublishContext<TMessage>>, IFilter<SendContext<TMessage>>
        where TMessage : class
    {
        private const int maxMessageSize = 256 * 1024;
        private const int messageOverhead = 3 * 1024;

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope($"message-size-{typeof(TMessage)}");
        }

        public async Task Send(PublishContext<TMessage> context, IPipe<PublishContext<TMessage>> next)
        {
            CheckMessageSize(context.Message);

            await next.Send(context);
        }

        public async Task Send(SendContext<TMessage> context, IPipe<SendContext<TMessage>> next)
        {
            CheckMessageSize(context.Message);

            await next.Send(context);
        }

        private static void CheckMessageSize(TMessage message)
        {
            var jsonString = JsonSerializer.Serialize(message);
            if (jsonString.Length + messageOverhead > maxMessageSize)
            {
                throw new ExcessiveMessageSizeException(typeof(TMessage), maxMessageSize - messageOverhead, jsonString.Length);
            }
        }
    }
}

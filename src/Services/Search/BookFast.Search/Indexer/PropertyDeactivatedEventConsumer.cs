using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class PropertyDeactivatedEventConsumer(PropertyProjection projection) : IConsumer<PropertyDeactivatedEvent>
    {
        public async Task Consume(ConsumeContext<PropertyDeactivatedEvent> context)
        {
            var message = context.Message;

            var applied = await projection.ApplyDeactivatedAsync(message.PropertyId, message.OccurredAt, context.CancellationToken);

            if (applied)
            {
                await context.Send(new ReindexPropertyAccommodations { PropertyId = message.PropertyId }, context.CancellationToken);
            }
        }
    }
}

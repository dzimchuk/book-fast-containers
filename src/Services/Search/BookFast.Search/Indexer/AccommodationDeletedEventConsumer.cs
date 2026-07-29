using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationDeletedEventConsumer(AccommodationIndex index) : IConsumer<AccommodationDeletedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationDeletedEvent> context)
        {
            var message = context.Message;

            return index.DeleteAsync(message.AccommodationId, message.TenantId, message.PropertyId, message.OccurredAt, context.CancellationToken);
        }
    }
}

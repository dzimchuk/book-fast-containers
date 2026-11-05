using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationCreatedEventConsumer(AccommodationIndex index) : IConsumer<AccommodationCreatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationCreatedEvent> context)
        {
            var message = context.Message;

            return index.UpsertAsync(new AccommodationIndexRecord
            {
                AccommodationId = message.AccommodationId,
                TenantId = message.TenantId,
                PropertyId = message.PropertyId.ToString(),
                Name = message.Name,
                Description = message.Description,
                Bedrooms = message.Bedrooms,
                OwnImages = message.Images ?? [],
                OwnFacilities = message.Facilities ?? [],
                AccommodationOccurredAt = message.OccurredAt,
            }, context.CancellationToken);
        }
    }
}

using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationUpdatedEventConsumer(AccommodationIndex index) : IConsumer<AccommodationUpdatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationUpdatedEvent> context)
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
                MinPriceAmount = (double?)message.PriceRange?.MinPrice?.Amount,
                MinPriceCurrency = message.PriceRange?.MinPrice?.Currency,
                MaxPriceAmount = (double?)message.PriceRange?.MaxPrice?.Amount,
                MaxPriceCurrency = message.PriceRange?.MaxPrice?.Currency,
                AccommodationOccurredAt = message.OccurredAt,
            }, context.CancellationToken);
        }
    }
}

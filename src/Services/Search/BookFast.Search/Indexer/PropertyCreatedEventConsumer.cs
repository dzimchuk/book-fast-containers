using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class PropertyCreatedEventConsumer(PropertyProjection projection) : IConsumer<PropertyCreatedEvent>
    {
        public async Task Consume(ConsumeContext<PropertyCreatedEvent> context)
        {
            var message = context.Message;

            var applied = await projection.ApplyAsync(new PropertyProjectionRecord
            {
                PropertyId = message.PropertyId,
                Name = message.Name,
                Description = message.Description,
                City = message.Address?.City,
                Country = message.Address?.Country,
                Latitude = message.Location?.Latitude,
                Longitude = message.Location?.Longitude,
                Facilities = message.Facilities ?? [],
                Images = message.Images ?? [],
                OccurredAt = message.OccurredAt,
            }, context.CancellationToken);

            if (applied)
            {
                await context.Send(new ReindexPropertyAccommodations { PropertyId = message.PropertyId }, context.CancellationToken);
            }
        }
    }
}

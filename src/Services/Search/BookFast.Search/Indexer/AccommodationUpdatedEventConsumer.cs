using BookFast.PropertyManagement.Integration;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationUpdatedEventConsumer(ILogger<AccommodationUpdatedEventConsumer> logger) : IConsumer<AccommodationUpdatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationUpdatedEvent> context)
        {
            logger.LogInformation("Accommodation updated: {AccommodationId}", context.Message.AccommodationId);
            return Task.CompletedTask;
        }
    }
}

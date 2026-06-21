using BookFast.PropertyManagement.Integration;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationCreatedEventConsumer(ILogger<AccommodationCreatedEventConsumer> logger) : IConsumer<AccommodationCreatedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationCreatedEvent> context)
        {
            logger.LogInformation("Accommodation created: {AccommodationId}", context.Message.AccommodationId);
            return Task.CompletedTask;
        }
    }
}

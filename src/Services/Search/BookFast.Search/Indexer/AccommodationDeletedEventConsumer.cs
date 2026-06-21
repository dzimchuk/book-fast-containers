using BookFast.PropertyManagement.Integration;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationDeletedEventConsumer(ILogger<AccommodationDeletedEventConsumer> logger) : IConsumer<AccommodationDeletedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationDeletedEvent> context)
        {
            logger.LogInformation("Accommodation deleted: {AccommodationId}", context.Message.AccommodationId);
            return Task.CompletedTask;
        }
    }
}

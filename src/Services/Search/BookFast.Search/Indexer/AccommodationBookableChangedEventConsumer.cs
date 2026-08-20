using BookFast.Booking.Integration;
using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class AccommodationBookableChangedEventConsumer(AccommodationIndex index) : IConsumer<AccommodationBookableChanged>
    {
        public Task Consume(ConsumeContext<AccommodationBookableChanged> context)
        {
            var message = context.Message;

            return index.ApplyBookableChangedAsync(message.AccommodationId, message.Bookable, message.Rate, message.OccurredAt, context.CancellationToken);
        }
    }
}

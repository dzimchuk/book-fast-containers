using BookFast.Search.Store;
using MassTransit;

namespace BookFast.Search.Indexer
{
    internal class ReindexPropertyAccommodationsConsumer(AccommodationIndex index) : IConsumer<ReindexPropertyAccommodations>
    {
        public Task Consume(ConsumeContext<ReindexPropertyAccommodations> context) =>
            index.ReindexPropertyAsync(context.Message.PropertyId, context.CancellationToken);
    }
}

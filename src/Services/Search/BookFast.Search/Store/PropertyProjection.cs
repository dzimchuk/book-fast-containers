using Microsoft.Extensions.VectorData;

namespace BookFast.Search.Store
{
    internal class PropertyProjection(VectorStoreCollection<Guid, PropertyProjectionRecord> collection)
    {
        public async Task<bool> ApplyAsync(PropertyProjectionRecord record, CancellationToken cancellationToken = default)
        {
            var existing = await collection.GetAsync(record.PropertyId, cancellationToken: cancellationToken);

            if (existing is not null && existing.OccurredAt >= record.OccurredAt)
            {
                return false;
            }

            await collection.UpsertAsync(record, cancellationToken);

            return true;
        }

        public async Task<bool> ApplyDeactivatedAsync(Guid propertyId, DateTimeOffset occurredAt, CancellationToken cancellationToken = default)
        {
            var existing = await collection.GetAsync(propertyId, cancellationToken: cancellationToken);

            if (existing is not null && existing.OccurredAt >= occurredAt)
            {
                return false;
            }

            var record = existing ?? new PropertyProjectionRecord { PropertyId = propertyId, Name = string.Empty };

            record.Active = false;
            record.OccurredAt = occurredAt;

            await collection.UpsertAsync(record, cancellationToken);

            return true;
        }
    }
}

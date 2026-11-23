using BookFast.Common.Domain;
using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;

namespace BookFast.Search.Tests.Search
{
    public class PurgeTests(SearchFixture fixture) : IClassFixture<SearchFixture>, IAsyncLifetime
    {
        public ValueTask InitializeAsync() => new(fixture.ResetAsync());

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        private readonly DateTimeOffset occurredAt = DateTimeOffset.UtcNow;

        private async Task<Guid> CreateAndTombstoneAsync()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var tenantId = "tenant-1";

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId,
                Name = "ToPurge",
                Bedrooms = 1,
                Images = [],
                Quantity = 1,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                Facilities = [],
                OccurredAt = occurredAt.AddSeconds(-1)
            });

            await fixture.WaitForAccommodationRecordAsync(accommodationId, r => r is not null);

            await fixture.PublishAsync(new AccommodationDeletedEvent
            {
                AccommodationId = accommodationId,
                TenantId = tenantId,
                PropertyId = propertyId,
                OccurredAt = occurredAt,
            });

            await fixture.WaitForAccommodationRecordAsync(accommodationId, r => r is not null && !r.Active);

            return accommodationId;
        }

        [Fact]
        public async Task Purge_TombstonedRecordOlderThanCutoff_IsPermanentlyRemoved()
        {
            var accommodationId = await CreateAndTombstoneAsync();

            await Task.Delay(1000, TestContext.Current.CancellationToken);

            await fixture.PurgeAsync(occurredAt.AddSeconds(1), TestContext.Current.CancellationToken);

            var record = await fixture.GetRecordAsync<AccommodationIndexRecord>(accommodationId, TestContext.Current.CancellationToken);

            Assert.Null(record);
        }

        [Fact]
        public async Task Purge_TombstonedRecordYoungerThanCutoff_IsLeftInPlace()
        {
            var accommodationId = await CreateAndTombstoneAsync();

            await Task.Delay(1000, TestContext.Current.CancellationToken);

            await fixture.PurgeAsync(occurredAt.AddSeconds(-1), TestContext.Current.CancellationToken);

            var record = await fixture.GetRecordAsync<AccommodationIndexRecord>(accommodationId, TestContext.Current.CancellationToken);

            Assert.NotNull(record);
            Assert.False(record.Active);
        }
    }
}

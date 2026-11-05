using BookFast.Common.Domain;
using BookFast.PropertyManagement.Integration;
using BookFast.Search.Store;

namespace BookFast.Search.Tests.Search
{
    [Collection(nameof(IntegrationTestCollection))]
    public class PurgeTests(SearchFixture fixture) : IClassFixture<SearchFixture>, IAsyncLifetime
    {
        public Task InitializeAsync() => fixture.ResetAsync();

        public Task DisposeAsync() => Task.CompletedTask;

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

            await Task.Delay(1000);

            await fixture.PurgeAsync(occurredAt.AddSeconds(1));

            var record = await fixture.GetRecordAsync<AccommodationIndexRecord>(accommodationId);

            Assert.Null(record);
        }

        [Fact]
        public async Task Purge_TombstonedRecordYoungerThanCutoff_IsLeftInPlace()
        {
            var accommodationId = await CreateAndTombstoneAsync();

            await Task.Delay(1000);

            await fixture.PurgeAsync(occurredAt.AddSeconds(-1));

            var record = await fixture.GetRecordAsync<AccommodationIndexRecord>(accommodationId);

            Assert.NotNull(record);
            Assert.False(record.Active);
        }
    }
}

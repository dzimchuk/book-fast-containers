using BookFast.Booking.Integration;
using BookFast.Common.Domain;
using BookFast.PropertyManagement.Integration;

namespace BookFast.Booking.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class AccommodationProjectionTests(AccommodationProjectionFixture fixture) : IClassFixture<AccommodationProjectionFixture>
    {
        private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

        [Fact]
        public async Task AccommodationCreated_WithPriceRange_SeedsRateBookableAndPublishes()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var occurredAt = Now;

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = occurredAt,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            Assert.Equal(5, accommodation.Quantity);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
            Assert.True(accommodation.Bookable);

            var published = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationBookableChanged>(e => e.AccommodationId == accommodationId);

            Assert.True(published.Bookable);
            Assert.Equal(new Money(100m, "USD"), published.Rate);
        }

        [Fact]
        public async Task AccommodationCreated_WithOnlyMaxPrice_SeedsRateFromMaxPrice()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 2,
                PriceRange = new PriceRange(null, new Money(200m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = Now,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            Assert.Equal(new Money(200m, "USD"), accommodation.Rate);
        }

        [Fact]
        public async Task AccommodationCreated_NoPriceRange_RecordsNotBookableWithNoRate()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 3,
                PriceRange = null,
                Facilities = [],
                Images = [],
                OccurredAt = Now,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => a.Quantity == 3);

            Assert.Null(accommodation.Rate);
            Assert.False(accommodation.Bookable);
        }

        [Fact]
        public async Task AccommodationUpdated_RateAlreadySeeded_RefreshesQuantityWithoutClobberingRate()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 9,
                PriceRange = new PriceRange(new Money(999m, "USD"), new Money(999m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t2,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => a.Quantity == 9);

            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
        }

        [Fact]
        public async Task AccommodationUpdated_RateWasUnset_BackfillsRateAndPublishesBookableChanged()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 3,
                PriceRange = null,
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await fixture.WaitForAccommodationAsync(accommodationId, a => a.Quantity == 3);

            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 3,
                PriceRange = new PriceRange(new Money(120m, "USD"), new Money(120m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t2,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            Assert.Equal(new Money(120m, "USD"), accommodation.Rate);

            var published = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationBookableChanged>(e => e.AccommodationId == accommodationId && e.Bookable);

            Assert.Equal(new Money(120m, "USD"), published.Rate);
        }

        [Fact]
        public async Task AccommodationDeleted_MarksNotBookableAndPublishes()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            await fixture.PublishAsync(new AccommodationDeletedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                OccurredAt = t2,
            });

            var accommodation = await fixture.WaitForAccommodationAsync(accommodationId, a => !a.Bookable);

            Assert.False(accommodation.Bookable);

            var published = await fixture.IntegrationEvents.WaitForEventAsync<AccommodationBookableChanged>(e => e.AccommodationId == accommodationId && !e.Bookable);

            Assert.False(published.Bookable);
        }

        [Fact]
        public async Task PropertyDeactivated_MarksEveryAccommodationUnderThePropertyNotBookable()
        {
            var propertyId = Guid.NewGuid();
            var accommodationId1 = Guid.NewGuid();
            var accommodationId2 = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId1,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite 1",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId2,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite 2",
                Quantity = 2,
                PriceRange = new PriceRange(new Money(200m, "USD"), new Money(250m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await fixture.WaitForAccommodationAsync(accommodationId1, a => a.Bookable);
            await fixture.WaitForAccommodationAsync(accommodationId2, a => a.Bookable);

            await fixture.PublishAsync(new PropertyDeactivatedEvent
            {
                TenantId = "tenant-1",
                PropertyId = propertyId,
                OccurredAt = t2,
            });

            await fixture.WaitForAccommodationAsync(accommodationId1, a => !a.Bookable);
            await fixture.WaitForAccommodationAsync(accommodationId2, a => !a.Bookable);

            await fixture.IntegrationEvents.WaitForEventAsync<AccommodationBookableChanged>(e => e.AccommodationId == accommodationId1 && !e.Bookable);
            await fixture.IntegrationEvents.WaitForEventAsync<AccommodationBookableChanged>(e => e.AccommodationId == accommodationId2 && !e.Bookable);
        }

        [Fact]
        public async Task OlderEvent_ArrivingAfterANewerOne_IsIgnored()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t2,
            });

            await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            // an older, out-of-order update must never move state back
            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 999,
                PriceRange = new PriceRange(new Money(1m, "USD"), new Money(1m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            // give the (no-op) consumer time to run, then assert state never changed
            await Task.Delay(TimeSpan.FromSeconds(3));
            var accommodation = await fixture.GetAccommodationAsync(accommodationId);

            Assert.Equal(5, accommodation.Quantity);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
        }

        [Fact]
        public async Task AccommodationUpdated_ArrivingBeforeItsCreate_StillProducesCorrectRow()
        {
            var accommodationId = Guid.NewGuid();
            var propertyId = Guid.NewGuid();
            var t1 = Now;
            var t2 = t1.AddSeconds(1);

            // the update (occurring later, t2) is delivered first - no row exists yet
            await fixture.PublishAsync(new AccommodationUpdatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 5,
                PriceRange = new PriceRange(new Money(100m, "USD"), new Money(150m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t2,
            });

            await fixture.WaitForAccommodationAsync(accommodationId, a => a.Bookable);

            // the create (occurring earlier, t1) then arrives - must be rejected as stale
            await fixture.PublishAsync(new AccommodationCreatedEvent
            {
                AccommodationId = accommodationId,
                TenantId = "tenant-1",
                PropertyId = propertyId,
                Name = "Suite",
                Quantity = 1,
                PriceRange = new PriceRange(new Money(1m, "USD"), new Money(1m, "USD")),
                Facilities = [],
                Images = [],
                OccurredAt = t1,
            });

            await Task.Delay(TimeSpan.FromSeconds(3));
            var accommodation = await fixture.GetAccommodationAsync(accommodationId);

            Assert.Equal(5, accommodation.Quantity);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
        }
    }
}

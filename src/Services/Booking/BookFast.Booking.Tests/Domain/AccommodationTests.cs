using BookFast.Booking.Domain;
using BookFast.Common.Domain;

namespace BookFast.Booking.Tests.Domain
{
    public class AccommodationTests
    {
        private static readonly Guid AccommodationId = Guid.NewGuid();
        private static readonly Guid PropertyId = Guid.NewGuid();
        private const string TenantId = "tenant-1";

        private static readonly DateTimeOffset T1 = DateTimeOffset.UtcNow;
        private static readonly DateTimeOffset T2 = T1.AddMinutes(1);
        private static readonly DateTimeOffset T3 = T1.AddMinutes(2);

        [Fact]
        public void ApplyCreated_WithMinPrice_SeedsRateAndBecomesBookable()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);

            var changed = accommodation.ApplyCreated(5, new Money(100m, "USD"), T1);

            Assert.True(changed);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
            Assert.True(accommodation.Bookable);
            Assert.Equal(5, accommodation.Quantity);
        }

        [Fact]
        public void ApplyCreated_WithNoRate_StaysNotBookable()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);

            var changed = accommodation.ApplyCreated(5, null, T1);

            Assert.False(changed);
            Assert.Null(accommodation.Rate);
            Assert.False(accommodation.Bookable);
        }

        [Fact]
        public void ApplyUpdated_RateAlreadySeeded_DoesNotClobber()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, new Money(100m, "USD"), T1);

            var changed = accommodation.ApplyUpdated(9, new Money(999m, "USD"), T2);

            Assert.False(changed);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
            Assert.Equal(9, accommodation.Quantity);
        }

        [Fact]
        public void ApplyUpdated_RateUnset_BackfillsAndBecomesBookable()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, null, T1);

            var changed = accommodation.ApplyUpdated(5, new Money(150m, "USD"), T2);

            Assert.True(changed);
            Assert.Equal(new Money(150m, "USD"), accommodation.Rate);
            Assert.True(accommodation.Bookable);
        }

        [Fact]
        public void ApplyUpdated_BeforeCreate_StillProducesCorrectRow()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);

            var updateApplied = accommodation.ApplyUpdated(5, new Money(100m, "USD"), T2);
            var createApplied = accommodation.ApplyCreated(5, new Money(100m, "USD"), T1); // older, out-of-order

            Assert.True(updateApplied);
            Assert.False(createApplied);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
            Assert.True(accommodation.Bookable);
        }

        [Fact]
        public void Apply_OlderThanWatermark_IsIgnored()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, new Money(100m, "USD"), T2);

            var changed = accommodation.ApplyUpdated(1, new Money(1m, "USD"), T1);

            Assert.False(changed);
            Assert.Equal(5, accommodation.Quantity);
            Assert.Equal(new Money(100m, "USD"), accommodation.Rate);
        }

        [Fact]
        public void Apply_DuplicateSameTimestamp_IsIgnored()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, new Money(100m, "USD"), T1);

            var changed = accommodation.ApplyCreated(5, new Money(100m, "USD"), T1);

            Assert.False(changed);
        }

        [Fact]
        public void MarkNotBookable_WhenPreviouslyBookable_FlipsBookableOff()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, new Money(100m, "USD"), T1);

            var changed = accommodation.MarkNotBookable(T2);

            Assert.True(changed);
            Assert.False(accommodation.Bookable);
        }

        [Fact]
        public void MarkNotBookable_WhenNeverBookable_ReportsNoChange()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, null, T1);

            var changed = accommodation.MarkNotBookable(T2);

            Assert.False(changed);
            Assert.False(accommodation.Bookable);
        }

        [Fact]
        public void MarkNotBookable_OlderThanWatermark_IsIgnored()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            accommodation.ApplyCreated(5, new Money(100m, "USD"), T3);

            var changed = accommodation.MarkNotBookable(T2);

            Assert.False(changed);
            Assert.True(accommodation.Bookable);
        }

        [Fact]
        public void Book_IncrementsBookCount()
        {
            var accommodation = Accommodation.NewAccommodation(AccommodationId, TenantId, PropertyId);
            var initialBookCount = accommodation.BookCount;

            accommodation.Book();

            Assert.Equal(initialBookCount + 1, accommodation.BookCount);
        }
    }
}

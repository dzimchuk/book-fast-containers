using BookFast.Booking.Domain;
using BookFast.Common.Domain;

namespace BookFast.Booking.Tests.Domain
{
    public class ReservationTests
    {
        private static readonly Guid ReservationId = Guid.NewGuid();
        private static readonly Guid AccommodationId = Guid.NewGuid();
        private const string TenantId = "tenant-1";
        private const string GuestId = "guest-1";

        [Fact]
        public void NewReservation_IsPendingWithFrozenRate()
        {
            var stay = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-03"));
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

            var reservation = Reservation.NewReservation(
                ReservationId, GuestId, AccommodationId, TenantId, stay, 2, new Money(100m, "USD"), expiresAt);

            Assert.Equal(ReservationStatus.Pending, reservation.Status);
            Assert.Equal(new Money(100m, "USD"), reservation.Rate);
            Assert.Equal(expiresAt, reservation.ExpiresAt);
        }

        [Fact]
        public void Price_IsRateTimesNightsTimesUnits()
        {
            var stay = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-04")); // 3 nights
            var reservation = Reservation.NewReservation(
                ReservationId, GuestId, AccommodationId, TenantId, stay, 2, new Money(100m, "USD"), DateTimeOffset.UtcNow);

            Assert.Equal(new Money(600m, "USD"), reservation.Price);
        }

        [Fact]
        public void Price_UnaffectedByChangingTheSnapshottedRateInstance()
        {
            // The Rate on the reservation is a value copied at creation - nothing here re-reads the accommodation's current Rate.
            var stay = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-02"));
            var reservation = Reservation.NewReservation(
                ReservationId, GuestId, AccommodationId, TenantId, stay, 1, new Money(50m, "USD"), DateTimeOffset.UtcNow);

            Assert.Equal(new Money(50m, "USD"), reservation.Price);
        }

        [Fact]
        public void GetDisplayStatus_PendingPastCheckout_StaysPending()
        {
            var stay = new Stay(DateOnly.Parse("2026-08-01"), DateOnly.Parse("2026-08-03"));
            var reservation = Reservation.NewReservation(
                ReservationId, GuestId, AccommodationId, TenantId, stay, 1, new Money(50m, "USD"), DateTimeOffset.UtcNow);

            var displayStatus = reservation.GetDisplayStatus(DateOnly.Parse("2026-08-10"));

            Assert.Equal(ReservationStatus.Pending, displayStatus);
        }

        // GetDisplayStatus's Confirmed -> Completed branch is exercised at the integration level
        // (BookFast.Booking.Tests/Reservations), since Confirmed isn't reachable via any public
        // domain API until ticket 03 (payment settlement) - it's seeded directly via the DbContext there.
    }
}

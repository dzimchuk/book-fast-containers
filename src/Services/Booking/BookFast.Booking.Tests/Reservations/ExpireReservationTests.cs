using BookFast.Booking.Domain;
using BookFast.Common.Domain;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ExpireReservationTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
    {
        [Fact]
        public async Task Sweep_PendingReservationPastExpiresAt_IsExpiredAndUnitsFreeUp()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1,
                rate: new Money(100m, "USD"), expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            await fixture.TriggerExpirationSweepAsync();

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(ReservationStatus.Expired, reservation.Status);

            // The single unit is no longer held, so the same dates can now be reserved again.
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-01",
                CheckOut = "2026-09-03",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Sweep_PendingReservationNotYetExpired_IsLeftUntouched()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1,
                rate: new Money(100m, "USD"), expiresAt: DateTimeOffset.UtcNow.AddMinutes(15));

            await fixture.TriggerExpirationSweepAsync();

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(ReservationStatus.Pending, reservation.Status);
        }

        [Theory]
        [InlineData(ReservationStatus.Confirmed)]
        [InlineData(ReservationStatus.Cancelled)]
        public async Task Sweep_NeverAffectsConfirmedOrCancelledReservations(ReservationStatus status)
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1,
                rate: new Money(100m, "USD"), status: status, expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            await fixture.TriggerExpirationSweepAsync();

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(status, reservation.Status);
        }

        [Fact]
        public async Task Sweep_RunTwice_IsIdempotent()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1,
                rate: new Money(100m, "USD"), expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            await fixture.TriggerExpirationSweepAsync();
            await fixture.TriggerExpirationSweepAsync();

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(ReservationStatus.Expired, reservation.Status);
        }
    }
}

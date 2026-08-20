using BookFast.Booking.Domain;
using BookFast.Common.Domain;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CancelReservationTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
    {
        [Fact]
        public async Task Cancel_ConfirmedReservation_BecomesCancelledAndUnitsFreeUp()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1,
                rate: new Money(100m, "USD"), status: ReservationStatus.Confirmed);

            var cancelResponse = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/cancel", null);
            Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

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
        public async Task Cancel_AnotherGuestsReservation_ReturnsNotFound()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId, DateOnly.Parse("2026-09-10"), DateOnly.Parse("2026-09-12"), units: 1,
                rate: new Money(100m, "USD"), status: ReservationStatus.Confirmed);

            var response = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/cancel", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_UnknownReservation_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.PostAsync($"/api/reservations/{Guid.NewGuid()}/cancel", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(ReservationStatus.Pending)]
        [InlineData(ReservationStatus.Cancelled)]
        [InlineData(ReservationStatus.Expired)]
        public async Task Cancel_NonConfirmedReservation_ReturnsConflict(ReservationStatus status)
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-20"), DateOnly.Parse("2026-09-22"), units: 1,
                rate: new Money(100m, "USD"), status: status);

            var response = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/cancel", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Cancel_ConfirmedReservationPastCheckout_ReturnsConflict()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2020-01-01"), DateOnly.Parse("2020-01-03"), units: 1,
                rate: new Money(100m, "USD"), status: ReservationStatus.Confirmed);

            var response = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/cancel", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
    }
}

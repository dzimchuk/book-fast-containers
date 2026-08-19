using BookFast.Booking.Domain;
using BookFast.Common.Domain;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class ReservationVisibilityTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
    {
        [Fact]
        public async Task List_OnlyReturnsMyOwnReservations()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));

            var mineId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2027-01-01"), DateOnly.Parse("2027-01-03"), 1, new Money(100m, "USD"));

            var othersId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId, DateOnly.Parse("2027-01-05"), DateOnly.Parse("2027-01-07"), 1, new Money(100m, "USD"));

            var reservations = await fixture.HttpClient.GetFromJsonAsync<JsonElement[]>("/api/reservations");

            Assert.Contains(reservations, r => r.GetProperty("id").GetGuid() == mineId);
            Assert.DoesNotContain(reservations, r => r.GetProperty("id").GetGuid() == othersId);
        }

        [Fact]
        public async Task Get_MyOwnReservation_ReturnsIt()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2027-02-01"), DateOnly.Parse("2027-02-03"), 1, new Money(100m, "USD"));

            var response = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal(reservationId, payload.GetProperty("id").GetGuid());
        }

        [Fact]
        public async Task Get_AnotherGuestsReservation_ReturnsNotFound()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId, DateOnly.Parse("2027-03-01"), DateOnly.Parse("2027-03-03"), 1, new Money(100m, "USD"));

            var response = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_UnknownReservation_ReturnsNotFound()
        {
            var response = await fixture.HttpClient.GetAsync($"/api/reservations/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_ConfirmedReservationPastCheckout_ShowsAsCompleted()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2020-01-01"), DateOnly.Parse("2020-01-03"), 1, new Money(100m, "USD"),
                status: ReservationStatus.Confirmed);

            var response = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal("Completed", payload.GetProperty("status").GetString());
        }
    }
}

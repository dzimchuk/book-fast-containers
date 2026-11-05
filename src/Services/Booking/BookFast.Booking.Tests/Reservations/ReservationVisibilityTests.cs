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
                accommodationId, ReservationsFixture.GuestId, StayDates.Upcoming(1), StayDates.Upcoming(3), 1, new Money(100m, "USD"));

            var othersId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId, StayDates.Upcoming(5), StayDates.Upcoming(7), 1, new Money(100m, "USD"));

            var reservations = await fixture.HttpClient.GetFromJsonAsync<JsonElement[]>("/api/reservations");

            Assert.Contains(reservations, r => r.GetProperty("id").GetGuid() == mineId);
            Assert.DoesNotContain(reservations, r => r.GetProperty("id").GetGuid() == othersId);
        }

        [Fact]
        public async Task Get_MyOwnReservation_ReturnsIt()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, StayDates.Upcoming(10), StayDates.Upcoming(12), 1, new Money(100m, "USD"));

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
                accommodationId, ReservationsFixture.OtherGuestId, StayDates.Upcoming(20), StayDates.Upcoming(22), 1, new Money(100m, "USD"));

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
                accommodationId, ReservationsFixture.GuestId, StayDates.Past(30), StayDates.Past(28), 1, new Money(100m, "USD"),
                status: ReservationStatus.Confirmed);

            var response = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal("Completed", payload.GetProperty("status").GetString());
        }
    }
}

using BookFast.Common.Domain;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CreateReservationTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
    {
        [Fact]
        public async Task Create_ForBookableAccommodation_ReturnsPendingWithFrozenPriceAndExpiry()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));

            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-01",
                CheckOut = "2026-09-04", // 3 nights
                Units = 2
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

            Assert.Equal("Pending", payload.GetProperty("status").GetString());
            Assert.Equal(600m, decimal.Parse(payload.GetProperty("price").GetProperty("amount").GetString()));
            Assert.Equal("USD", payload.GetProperty("price").GetProperty("currency").GetString());
            Assert.True(payload.GetProperty("expiresAt").GetDateTimeOffset() > DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task Create_AgainstNotBookableAccommodation_IsRejected()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: null);

            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-01",
                CheckOut = "2026-09-03",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_AgainstDeactivatedAccommodation_IsRejected()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"), active: false);

            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-01",
                CheckOut = "2026-09-03",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_WhenANightLacksEnoughFreeUnits_IsRejectedAllOrNothing()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 2, rate: new Money(100m, "USD"));

            // Holds both units for one night (Sep 2) in the middle of the requested stay.
            await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId,
                DateOnly.Parse("2026-09-02"), DateOnly.Parse("2026-09-03"), units: 2, rate: new Money(100m, "USD"));

            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-01",
                CheckOut = "2026-09-04",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var reservations = await fixture.HttpClient.GetFromJsonAsync<JsonElement[]>("/api/reservations");
            Assert.DoesNotContain(reservations, r => r.GetProperty("accommodationId").GetGuid() == accommodationId);
        }

        [Fact]
        public async Task Create_SameDayCheckoutCheckin_DoesNotConflict()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));

            await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId,
                DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"));

            // Checks in the very day the other reservation checks out - must not conflict.
            var response = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-09-03",
                CheckOut = "2026-09-05",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Create_TwoConcurrentCreatesForTheLastUnit_YieldsExactlyOneSuccess()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 1, rate: new Money(100m, "USD"));

            var requestBody = new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-10-01",
                CheckOut = "2026-10-03",
                Units = 1
            };

            var responses = await Task.WhenAll(
                fixture.HttpClient.PostAsJsonAsync("/api/reservations", requestBody),
                fixture.HttpClient.PostAsJsonAsync("/api/reservations", requestBody));

            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            Assert.Single(responses, r => r.StatusCode != HttpStatusCode.Created);
        }

        [Fact]
        public async Task Create_LaterRateChangeDoesNotAffectAlreadyFrozenPrice()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));

            var createResponse = await fixture.HttpClient.PostAsJsonAsync("/api/reservations", new
            {
                AccommodationId = accommodationId,
                CheckIn = "2026-11-01",
                CheckOut = "2026-11-02",
                Units = 1
            });

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync()).RootElement;
            var reservationId = created.GetProperty("id").GetGuid();

            // Simulates a later catalogue-driven Rate change on the accommodation - must not move the existing Price.
            await fixture.SetAccommodationRateAsync(accommodationId, new Money(500m, "USD"));

            var getResponse = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");
            var fetched = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync()).RootElement;

            Assert.Equal(100m, decimal.Parse(fetched.GetProperty("price").GetProperty("amount").GetString()));
        }
    }
}

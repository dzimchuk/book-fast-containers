using BookFast.Booking.Domain;
using BookFast.Common.Domain;
using System.Net;
using System.Text.Json;

namespace BookFast.Booking.Tests.Reservations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class PayReservationTests(ReservationsFixture fixture) : IClassFixture<ReservationsFixture>
    {
        [Fact]
        public async Task Pay_Settle_ThenSweep_ConfirmsReservation()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"));

            var payResponse = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/pay", null);
            Assert.Equal(HttpStatusCode.Accepted, payResponse.StatusCode);

            await fixture.TriggerPaymentSweepAsync();

            // PaymentSettled travels over the real bus
            var reservation = await fixture.WaitForReservationAsync(reservationId, r => r.Status == ReservationStatus.Confirmed);

            Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        }

        [Fact]
        public async Task Pay_Decline_ThenSweep_LeavesReservationPending()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"));

            var payResponse = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/pay?outcome=Decline", null);
            Assert.Equal(HttpStatusCode.Accepted, payResponse.StatusCode);

            await fixture.TriggerPaymentSweepAsync();

            var getResponse = await fixture.HttpClient.GetAsync($"/api/reservations/{reservationId}");
            var payload = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync()).RootElement;
            Assert.Equal("Pending", payload.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Pay_AnotherGuestsReservation_ReturnsNotFound()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.OtherGuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"));

            var payResponse = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/pay", null);

            Assert.Equal(HttpStatusCode.NotFound, payResponse.StatusCode);
        }

        [Fact]
        public async Task Pay_NonPendingReservation_ReturnsConflict()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"),
                status: ReservationStatus.Confirmed);

            var payResponse = await fixture.HttpClient.PostAsync($"/api/reservations/{reservationId}/pay", null);

            Assert.Equal(HttpStatusCode.Conflict, payResponse.StatusCode);
        }

        [Fact]
        public async Task DuplicateSettlement_LeavesReservationConfirmed_NoSideEffects()
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"),
                status: ReservationStatus.Confirmed);

            await fixture.SendConfirmReservationDirectlyAsync(reservationId);
            await fixture.SendConfirmReservationDirectlyAsync(reservationId);

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        }

        [Theory]
        [InlineData(ReservationStatus.Expired)]
        [InlineData(ReservationStatus.Cancelled)]
        public async Task Settlement_ForNonPendingReservation_DoesNotResurrectOrCorrupt(ReservationStatus status)
        {
            var accommodationId = await fixture.SeedAccommodationAsync(quantity: 5, rate: new Money(100m, "USD"));
            var reservationId = await fixture.SeedReservationAsync(
                accommodationId, ReservationsFixture.GuestId, DateOnly.Parse("2026-09-01"), DateOnly.Parse("2026-09-03"), units: 1, rate: new Money(100m, "USD"),
                status: status);

            await fixture.SendConfirmReservationDirectlyAsync(reservationId);

            var reservation = await fixture.GetReservationAsync(reservationId);
            Assert.Equal(status, reservation.Status);
        }
    }
}

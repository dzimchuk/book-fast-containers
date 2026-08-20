using BookFast.Booking.Application.Reservations.ConfirmReservation;
using BookFast.Booking.Domain;
using BookFast.Booking.Infrastructure.Database;
using BookFast.Booking.Infrastructure.Payments;
using BookFast.Booking.Infrastructure.Reservations;
using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookFast.Booking.Tests.Reservations
{
    public class ReservationsFixture : IAsyncLifetime
    {
        public const string GuestId = Constants.UserId;
        public const string OtherGuestId = "11111111-1111-1111-1111-111111111111";
        public const string TenantId = "tenant-1";

        private readonly WebApplicationFactory<Program> factory;
        private readonly IServiceScope scope;
        private readonly BookingContext dbContext;

        public HttpClient HttpClient { get; }

        public ReservationsFixture(BookingApiFixture fixture)
        {
            factory = fixture.GetWebApplicationFactory(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = GuestId,
                    Role = Roles.Customer
                });
            });

            HttpClient = factory.CreateClient();

            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<BookingContext>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            scope.Dispose();
            await factory.DisposeAsync();
        }

        public async Task<Guid> SeedAccommodationAsync(int quantity, Money rate, bool active = true, string tenantId = TenantId)
        {
            var occurredAt = DateTimeOffset.UtcNow;

            var accommodation = Accommodation.NewAccommodation(Guid.NewGuid(), tenantId, Guid.NewGuid());
            accommodation.ApplyCreated(quantity, rate, occurredAt);

            if (!active)
            {
                accommodation.MarkNotBookable(occurredAt.AddSeconds(1));
            }

            dbContext.Accommodations.Add(accommodation);
            await dbContext.SaveChangesAsync();

            return accommodation.Id;
        }

        public async Task<Guid> SeedReservationAsync(
            Guid accommodationId, string guestId, DateOnly checkIn, DateOnly checkOut, int units, Money rate,
            string tenantId = TenantId, ReservationStatus status = ReservationStatus.Pending, DateTimeOffset? expiresAt = null)
        {
            var reservation = Reservation.NewReservation(
                Guid.NewGuid(), guestId, accommodationId, tenantId, new Stay(checkIn, checkOut), units, rate, expiresAt ?? DateTimeOffset.UtcNow.AddMinutes(15));

            dbContext.Reservations.Add(reservation);
            await dbContext.SaveChangesAsync();

            if (status != ReservationStatus.Pending)
            {
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE booking.reservations SET status = {status.ToString()} WHERE id = {reservation.Id}");
            }

            return reservation.Id;
        }

        public Task<Reservation> GetReservationAsync(Guid reservationId) =>
            dbContext.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == reservationId);

        public async Task<Reservation> WaitForReservationAsync(Guid reservationId, Func<Reservation, bool> predicate, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

            while (DateTime.UtcNow < deadline)
            {
                var reservation = await GetReservationAsync(reservationId);
                if (reservation is not null && predicate(reservation))
                {
                    return reservation;
                }

                await Task.Delay(200);
            }

            throw new TimeoutException($"No reservation {reservationId} matching the predicate was found within the timeout.");
        }

        public Task SetAccommodationRateAsync(Guid accommodationId, Money rate) =>
            dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE booking.accommodations SET rate_amount = {rate.Amount}, rate_currency = {rate.Currency} WHERE id = {accommodationId}");

        public Task TriggerPaymentSweepAsync()
        {
            var sweepService = factory.Services.GetServices<IHostedService>().OfType<PaymentSettlementSweepService>().Single();

            return sweepService.RunOnceAsync(CancellationToken.None);
        }

        public Task TriggerExpirationSweepAsync()
        {
            var sweepService = factory.Services.GetServices<IHostedService>().OfType<ReservationExpirationSweepService>().Single();

            return sweepService.RunOnceAsync(CancellationToken.None);
        }

        public async Task SendConfirmReservationDirectlyAsync(Guid reservationId)
        {
            using var freshScope = factory.Services.CreateScope();
            var sender = freshScope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(new ConfirmReservationCommand { ReservationId = reservationId });
        }
    }
}

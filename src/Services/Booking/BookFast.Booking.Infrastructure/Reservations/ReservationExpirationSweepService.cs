using BookFast.Booking.Application;
using BookFast.Booking.Domain;
using BookFast.Common.Application.Clock;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookFast.Booking.Infrastructure.Reservations
{
    internal class ReservationExpirationSweepService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReservationExpirationOptions> options,
        ILogger<ReservationExpirationSweepService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(options.Value.SweepInterval);

            try
            {
                do
                {
                    await RunOnceAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Reservation expiration sweep is stopping.");
            }
        }

        internal async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
                var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

                await dbContext.ExecuteInTransactionAsync(ct => ExpireDueReservationsAsync(dbContext, dateTimeProvider, ct), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Reservation expiration sweep run failed.");
            }
        }

        private static async Task<Result> ExpireDueReservationsAsync(
            IDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
        {
            var now = new DateTimeOffset(dateTimeProvider.UtcNow);

            var dueReservations = await dbContext.Reservations
                .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt <= now)
                .ToListAsync(cancellationToken);

            foreach (var reservation in dueReservations)
            {
                reservation.Expire();
            }

            return Result.Success();
        }
    }
}

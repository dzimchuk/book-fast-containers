using BookFast.Booking.Application;
using BookFast.Common.Application.Clock;
using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookFast.Booking.Infrastructure.Payments
{
    internal class PaymentSettlementSweepService(
        IServiceScopeFactory scopeFactory,
        IOptions<PaymentOptions> options,
        ILogger<PaymentSettlementSweepService> logger) : BackgroundService
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
                logger.LogInformation("Payment settlement sweep is stopping.");
            }
        }

        internal async Task RunOnceAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
                var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
                var messageSender = scope.ServiceProvider.GetRequiredService<ILocalBrokeredMessageSender>();

                await dbContext.ExecuteInTransactionAsync(
                    ct => ProcessDueAttemptsAsync(dbContext, messageSender, dateTimeProvider, ct), cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Payment settlement sweep run failed.");
            }
        }

        private static async Task<Result> ProcessDueAttemptsAsync(
            IDbContext dbContext, ILocalBrokeredMessageSender messageSender, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
        {
            var now = new DateTimeOffset(dateTimeProvider.UtcNow);

            var dueAttempts = await dbContext.PaymentAttempts
                .Where(attempt => attempt.DueAt <= now)
                .ToListAsync(cancellationToken);

            foreach (var attempt in dueAttempts)
            {
                dbContext.PaymentAttempts.Remove(attempt);

                await messageSender.SendAsync(new PaymentSettled { ReservationId = attempt.ReservationId, OccurredAt = now }, cancellationToken);
            }

            return Result.Success();
        }
    }
}


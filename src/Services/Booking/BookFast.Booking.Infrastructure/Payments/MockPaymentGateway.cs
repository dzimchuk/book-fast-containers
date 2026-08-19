using BookFast.Booking.Application;
using BookFast.Booking.Application.Payments;
using BookFast.Booking.Domain;
using BookFast.Common.Application.Clock;
using Microsoft.Extensions.Options;

namespace BookFast.Booking.Infrastructure.Payments
{
    internal class MockPaymentGateway(IDbContext dbContext, IDateTimeProvider dateTimeProvider, IOptions<PaymentOptions> options) : IPaymentGateway
    {
        public Task ChargeAsync(Guid reservationId, CancellationToken cancellationToken)
        {
            var dueAt = new DateTimeOffset(dateTimeProvider.UtcNow).Add(options.Value.SettlementDelay);

            dbContext.PaymentAttempts.Add(PaymentAttempt.Schedule(Guid.CreateVersion7(), reservationId, dueAt));

            return Task.CompletedTask;
        }
    }
}

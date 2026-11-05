namespace BookFast.Booking.Infrastructure.Payments
{
    internal class PaymentOptions
    {
        public TimeSpan SettlementDelay { get; set; } = TimeSpan.FromSeconds(5);

        public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}

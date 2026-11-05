namespace BookFast.Booking.Infrastructure.Payments
{
    // Internal to Booking's payment emulator (not published cross-service) - per ADR-0007.
    internal record PaymentSettled
    {
        public Guid ReservationId { get; init; }

        public DateTimeOffset OccurredAt { get; init; }
    }
}

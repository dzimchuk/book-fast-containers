namespace BookFast.Booking.Infrastructure.Reservations
{
    internal class ReservationExpirationOptions
    {
        public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(2);
    }
}

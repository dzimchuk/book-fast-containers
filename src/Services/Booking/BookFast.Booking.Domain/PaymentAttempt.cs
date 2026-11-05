using BookFast.Common.Domain;

namespace BookFast.Booking.Domain
{
    public class PaymentAttempt : Entity<Guid>
    {
        public Guid ReservationId { get; private set; }

        public DateTimeOffset DueAt { get; private set; }

        public static PaymentAttempt Schedule(Guid id, Guid reservationId, DateTimeOffset dueAt)
        {
            return new PaymentAttempt
            {
                Id = id,
                ReservationId = reservationId,
                DueAt = dueAt
            };
        }
    }
}

using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.GetReservation
{
    public class GetReservationQuery : IQuery<ReservationRepresentation>
    {
        public Guid Id { get; set; }
    }
}

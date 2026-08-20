using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.CancelReservation
{
    public class CancelReservationCommand : ICommand<ReservationRepresentation>
    {
        public Guid ReservationId { get; set; }
    }
}

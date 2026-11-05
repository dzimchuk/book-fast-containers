using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.ConfirmReservation
{
    public class ConfirmReservationCommand : ICommand
    {
        public Guid ReservationId { get; set; }
    }
}

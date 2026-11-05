using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.CreateReservation
{
    public class CreateReservationCommand : ICommand<ReservationRepresentation>
    {
        public Guid AccommodationId { get; set; }

        public DateOnly CheckIn { get; set; }

        public DateOnly CheckOut { get; set; }

        public int Units { get; set; }
    }
}

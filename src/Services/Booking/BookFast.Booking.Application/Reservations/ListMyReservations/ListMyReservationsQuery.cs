using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.ListMyReservations
{
    public class ListMyReservationsQuery : IQuery<IReadOnlyList<ReservationRepresentation>>
    {
    }
}

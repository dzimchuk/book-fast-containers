using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.ListTenantReservations
{
    public class ListTenantReservationsQuery : IQuery<IReadOnlyList<ReservationRepresentation>>
    {
    }
}

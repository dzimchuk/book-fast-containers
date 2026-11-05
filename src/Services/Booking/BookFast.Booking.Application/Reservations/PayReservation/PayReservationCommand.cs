using BookFast.Booking.Application.Payments;
using BookFast.Common.Application.Messaging;

namespace BookFast.Booking.Application.Reservations.PayReservation
{
    public class PayReservationCommand : ICommand<ReservationRepresentation>
    {
        public Guid ReservationId { get; set; }

        public PaymentOutcome Outcome { get; set; } = PaymentOutcome.Settle;
    }
}

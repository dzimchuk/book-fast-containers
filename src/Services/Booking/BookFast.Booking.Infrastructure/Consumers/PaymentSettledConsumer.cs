using BookFast.Booking.Application.Reservations.ConfirmReservation;
using BookFast.Booking.Infrastructure.Payments;
using MassTransit;
using MediatR;

namespace BookFast.Booking.Infrastructure.Consumers
{
    internal class PaymentSettledConsumer(ISender sender) : IConsumer<PaymentSettled>
    {
        public Task Consume(ConsumeContext<PaymentSettled> context)
        {
            var command = new ConfirmReservationCommand { ReservationId = context.Message.ReservationId };

            return sender.Send(command, context.CancellationToken);
        }
    }
}

namespace BookFast.Booking.Application.Payments
{
    public interface IPaymentGateway
    {
        Task ChargeAsync(Guid reservationId, CancellationToken cancellationToken);
    }
}

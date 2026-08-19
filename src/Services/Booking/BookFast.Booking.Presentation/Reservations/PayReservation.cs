using BookFast.Booking.Application.Payments;
using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.PayReservation;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class PayReservation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("reservations/{reservationId}/pay", async (
                Guid reservationId,
                ISender sender,
                PaymentOutcome outcome = PaymentOutcome.Settle) =>
            {
                var result = await sender.Send(new PayReservationCommand { ReservationId = reservationId, Outcome = outcome });

                return result.Map(
                    reservation => Results.AcceptedAtRoute("GetReservation", new { reservationId = reservation.Id }, reservation),
                    ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.Customer)
            .Produces<ReservationRepresentation>(StatusCodes.Status202Accepted)
            .WithTags(Tags.Reservations);
        }
    }
}

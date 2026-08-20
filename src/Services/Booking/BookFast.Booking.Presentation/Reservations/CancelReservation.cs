using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.CancelReservation;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class CancelReservation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("reservations/{reservationId}/cancel", async (
                Guid reservationId,
                ISender sender) =>
            {
                var result = await sender.Send(new CancelReservationCommand { ReservationId = reservationId });

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.Customer)
            .Produces<ReservationRepresentation>(StatusCodes.Status200OK)
            .WithTags(Tags.Reservations);
        }
    }
}

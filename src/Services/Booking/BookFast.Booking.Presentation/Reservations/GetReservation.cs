using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.GetReservation;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class GetReservation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("reservations/{reservationId}", async (
                Guid reservationId,
                ISender sender) =>
            {
                var result = await sender.Send(new GetReservationQuery { Id = reservationId });

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.Customer)
            .Produces<ReservationRepresentation>(StatusCodes.Status200OK)
            .WithTags(Tags.Reservations)
            .WithName("GetReservation");
        }
    }
}

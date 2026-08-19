using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.CreateReservation;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class CreateReservation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("reservations", async (
                CreateReservationCommand request,
                ISender sender) =>
            {
                var result = await sender.Send(request);

                return result.Map(
                    reservation => Results.CreatedAtRoute("GetReservation", new { reservationId = reservation.Id }, reservation),
                    ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.Customer)
            .Produces<ReservationRepresentation>(StatusCodes.Status201Created)
            .WithTags(Tags.Reservations);
        }
    }
}

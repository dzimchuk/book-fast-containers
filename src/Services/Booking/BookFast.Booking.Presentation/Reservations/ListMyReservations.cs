using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.ListMyReservations;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class ListMyReservations : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("reservations", async (ISender sender) =>
            {
                var result = await sender.Send(new ListMyReservationsQuery());

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.Customer)
            .Produces<IReadOnlyList<ReservationRepresentation>>(StatusCodes.Status200OK)
            .WithTags(Tags.Reservations);
        }
    }
}

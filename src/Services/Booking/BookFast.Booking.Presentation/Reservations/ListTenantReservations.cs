using BookFast.Booking.Application.Reservations;
using BookFast.Booking.Application.Reservations.ListTenantReservations;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.Booking.Presentation.Reservations
{
    internal sealed class ListTenantReservations : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("reservations/provider", async (ISender sender) =>
            {
                var result = await sender.Send(new ListTenantReservationsQuery());

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<IReadOnlyList<ReservationRepresentation>>(StatusCodes.Status200OK)
            .WithTags(Tags.Reservations);
        }
    }
}

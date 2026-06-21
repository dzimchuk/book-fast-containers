using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Accommodations.DeleteAccommodation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Accommodations
{
    internal sealed class DeleteAccommodation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("properties/{propertyId}/accommodations/{accommodationId}", async (
                Guid propertyId,
                Guid accommodationId,
                ISender sender) =>
            {
                var result = await sender.Send(new DeleteAccommodationCommand { AccommodationId = accommodationId });

                return result.Map(() => Results.NoContent(), ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<string>(StatusCodes.Status204NoContent)
            .WithTags(Tags.Accommodations);
        }
    }
}

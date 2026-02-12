using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Accommodations;
using BookFast.PropertyManagement.Application.Accommodations.GetAccommodation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Accommodations
{
    internal sealed class GetAccommodation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("properties/{propertyId}/accommodations/{accommodationId}", async (
                int propertyId,
                int accommodationId,
                ISender sender) =>
            {
                var result = await sender.Send(new GetAccommodationQuery
                {
                    Id = accommodationId
                });

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<AccommodationRepresentation>(StatusCodes.Status200OK)
            .WithTags(Tags.Accommodations)
            .WithName("GetAccommodation");
        }
    }
}

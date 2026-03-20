using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Accommodations
{
    internal sealed class CreateAccommodation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("properties/{propertyId}/accommodations", async (
                Guid propertyId,
                CreateAccommodationCommand request,
                ISender sender) =>
            {
                request.PropertyId = propertyId;

                var result = await sender.Send(request);

                return result.Map(
                    accommodationId => Results.CreatedAtRoute("GetAccommodation", new { propertyId, accommodationId }, accommodationId),
                    ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<Guid>(StatusCodes.Status201Created)
            .WithTags(Tags.Accommodations);
        }
    }
}

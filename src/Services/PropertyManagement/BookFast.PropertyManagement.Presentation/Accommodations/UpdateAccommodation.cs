using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Accommodations
{
    internal sealed class UpdateAccommodation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("properties/{propertyId}/accommodations/{accommodationId}", async (
                int propertyId,
                int accommodationId,
                UpdateAccommodationCommand request,
                ISender sender) =>
            {
                request.AccommodationId = accommodationId;

                var updateResult = await sender.Send(request);

                return updateResult.Map(() => Results.NoContent(), ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<string>(StatusCodes.Status204NoContent)
            .WithTags(Tags.Accommodations);
        }
    }
}

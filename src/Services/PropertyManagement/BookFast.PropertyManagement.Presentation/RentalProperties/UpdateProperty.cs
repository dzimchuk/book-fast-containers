using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.RentalProperties
{
    internal sealed class UpdateProperty : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("properties/{propertyId}", async (
                Guid propertyId,
                UpdatePropertyCommand request,
                ISender sender) =>
            {
                request.PropertyId = propertyId;

                var updateResult = await sender.Send(request);

                return updateResult.Map(() => Results.NoContent(), ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<string>(StatusCodes.Status204NoContent)
            .WithTags(Tags.Properties);
        }
    }
}

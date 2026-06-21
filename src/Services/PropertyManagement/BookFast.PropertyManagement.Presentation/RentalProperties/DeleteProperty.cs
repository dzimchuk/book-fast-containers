using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.RentalProperties
{
    internal sealed class DeleteProperty : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("properties/{propertyId}", async (Guid propertyId, ISender sender) =>
            {
                var result = await sender.Send(new DeletePropertyCommand { PropertyId = propertyId });

                return result.Map(() => Results.NoContent(), ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<string>(StatusCodes.Status204NoContent)
            .WithTags(Tags.Properties);
        }
    }
}

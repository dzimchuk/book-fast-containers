using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.RentalProperties.CreateProperty;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.RentalProperties
{
    internal sealed class CreateProperty : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("properties", async (CreatePropertyCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);

                return result.Map(
                    propertyId => Results.CreatedAtRoute("GetProperty", new { propertyId }, propertyId),
                    ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<Guid>(StatusCodes.Status201Created)
            .WithTags(Tags.Properties);
        }
    }
}

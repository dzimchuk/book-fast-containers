using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.RentalProperties;
using BookFast.PropertyManagement.Application.RentalProperties.GetProperty;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.RentalProperties
{
    internal sealed class GetProperty : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("properties/{propertyId}", async (
                Guid propertyId,
                ISender sender) =>
            {
                var result = await sender.Send(new GetPropertyQuery
                {
                    Id = propertyId
                });

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<PropertyRepresentation>(StatusCodes.Status200OK)
            .WithTags(Tags.Properties)
            .WithName("GetProperty");
        }
    }
}

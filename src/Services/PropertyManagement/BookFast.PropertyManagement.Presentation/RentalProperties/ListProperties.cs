using BookFast.Common.Application.Queries;
using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.RentalProperties;
using BookFast.PropertyManagement.Application.RentalProperties.ListProperties;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.RentalProperties
{
    internal sealed class ListProperties : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("properties", async ([FromQuery(Name = "orderBy")] string orderBy,
                                            [FromQuery(Name = "orderDirection")] string orderDirection,
                                            [FromQuery(Name = "pageNumber")] int? pageNumber,
                                            [FromQuery(Name = "pageSize")] int? pageSize,
                                            ISender sender) =>
            {
                if (!Enum.TryParse<OrderDirection>(orderDirection, true, out var direction))
                {
                    direction = OrderDirection.Asc;
                }

                var result = await sender.Send(new ListPropertiesQuery
                {
                    OrderBy = orderBy,
                    OrderDirection = direction,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                });

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<ListQueryResult<PropertyRepresentation>>(StatusCodes.Status200OK)
            .WithTags(Tags.Properties);
        }
    }
}

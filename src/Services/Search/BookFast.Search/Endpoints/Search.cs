using BookFast.Common.Application.Queries;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.Common.SeedWork;
using Microsoft.AspNetCore.Mvc;

namespace BookFast.Search.Endpoints
{
    internal class Search : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("search", async (
                string query,
                [FromQuery(Name = "orderBy")] string orderBy,
                [FromQuery(Name = "orderDirection")] string orderDirection,
                [FromQuery(Name = "pageNumber")] int? pageNumber,
                [FromQuery(Name = "pageSize")] int? pageSize) =>
            {
                if (!Enum.TryParse<OrderDirection>(orderDirection, true, out var direction))
                {
                    direction = OrderDirection.Asc;
                }

                var result = Result.Success();

                return result.Map(() => Results.Ok(new ListQueryResult<SearchItem>()), ApiResults.Problem);
            })
            //.RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<ListQueryResult<SearchItem>>(StatusCodes.Status200OK)
            .WithTags("Search");
        }
    }

    internal class SearchItem
    {
    }
}

using BookFast.Common.Application.Queries;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Search.Store;
using Microsoft.AspNetCore.Mvc;

namespace BookFast.Search.Endpoints
{
    internal class Search : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("search", async (
                string query,
                [FromQuery(Name = "bedrooms")] int? bedrooms,
                [FromQuery(Name = "facilities")] string[] facilities,
                [FromQuery(Name = "city")] string city,
                [FromQuery(Name = "country")] string country,
                [FromQuery(Name = "sort")] string sort,
                [FromQuery(Name = "pageNumber")] int? pageNumber,
                [FromQuery(Name = "pageSize")] int? pageSize,
                AccommodationIndex store,
                CancellationToken cancellationToken) =>
            {
                if (!Enum.TryParse<SearchSort>(sort, true, out var sortBy))
                {
                    sortBy = SearchSort.Relevance;
                }

                var result = await store.SearchAsync(new SearchQuery
                {
                    Query = query,
                    Bedrooms = bedrooms,
                    Facilities = facilities ?? [],
                    City = city,
                    Country = country,
                    Sort = sortBy,
                    PageNumber = pageNumber is > 0 ? pageNumber.Value : 1,
                    PageSize = pageSize is > 0 ? pageSize.Value : 20,
                }, cancellationToken);

                return Results.Ok(result);
            })
            .Produces<ListQueryResult<SearchResult>>(StatusCodes.Status200OK)
            .WithTags("Search");
        }
    }
}


using Microsoft.AspNetCore.Routing;

namespace BookFast.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

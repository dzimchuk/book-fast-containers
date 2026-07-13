using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Replay;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Replay
{
    internal sealed class ReplayCatalogue : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("replay", async (ISender sender) =>
            {
                var result = await sender.Send(new ReplayCatalogueCommand());

                return result.Map(() => Results.Accepted(), ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdmin)
            .Produces(StatusCodes.Status202Accepted)
            .WithTags(Tags.Replay);
        }
    }
}

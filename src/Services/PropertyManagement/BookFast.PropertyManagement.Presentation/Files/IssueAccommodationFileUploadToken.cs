using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Endpoints;
using BookFast.Common.Presentation.Results;
using BookFast.PropertyManagement.Application.Files;
using BookFast.PropertyManagement.Application.Files.IssueFileUploadToken;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BookFast.PropertyManagement.Presentation.Files
{
    internal sealed class IssueAccommodationFileUploadToken : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("properties/{propertyId}/accommodations/{accommodationId}/upload-token", async (
                Guid propertyId,
                Guid accommodationId,
                string originalFileName,
                ISender sender) =>
            {
                var result = await sender.Send(new IssueFileUploadTokenCommand(propertyId, accommodationId, originalFileName));

                return result.Map(Results.Ok, ApiResults.Problem);
            })
            .RequireAuthorization(AuthorizationPolicies.TenantAdminOrUser)
            .Produces<FileAccessToken>(StatusCodes.Status200OK)
            .WithTags(Tags.Files);
        }
    }
}

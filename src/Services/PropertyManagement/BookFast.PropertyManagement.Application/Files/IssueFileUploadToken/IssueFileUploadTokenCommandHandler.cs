using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Files.IssueFileUploadToken
{
    internal sealed class IssueFileUploadTokenCommandHandler(IDbContext dbContext, ISecurityContext securityContext, IFileTokenIssuer fileTokenIssuer)
        : ICommandHandler<IssueFileUploadTokenCommand, FileAccessToken>
    {
        private const double TokenExpirationTime = 20;

        public async Task<Result<FileAccessToken>> Handle(IssueFileUploadTokenCommand request, CancellationToken cancellationToken)
        {
            if (!await dbContext.Properties.AnyAsync(p => p.TenantId == securityContext.GetCurrentTenant() && p.Id == request.PropertyId, cancellationToken))
            {
                return Result.Failure<FileAccessToken>(ErrorCodes.PropertyNotFound(request.PropertyId));
            }

            string path;

            if (request.AccommodationId.HasValue)
            {
                if (!await dbContext.Accommodations.AnyAsync(a => 
                    a.TenantId == securityContext.GetCurrentTenant() && a.Id == request.AccommodationId.Value && a.PropertyId == request.PropertyId, cancellationToken))
                {
                    return Result.Failure<FileAccessToken>(ErrorCodes.AccommodationNotFound(request.AccommodationId.Value));
                }

                var fileName = GenerateName(request.OriginalFileName);
                path = ConstructPath(request.PropertyId, request.AccommodationId.Value, fileName);
            }
            else
            {
                var fileName = GenerateName(request.OriginalFileName);
                path = ConstructPath(request.PropertyId, fileName);
            }

            var token = IssueImageUploadToken(path);
            return Result.Success(token);
        }

        private FileAccessToken IssueImageUploadToken(string path) =>
            fileTokenIssuer.IssueUploadToken(path, TimeSpan.FromMinutes(TokenExpirationTime));

        private static string GenerateName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".jpg";
            }

            return $"{Path.GetRandomFileName()}{extension}";
        }

        private static string ConstructPath(Guid propertyId, Guid accommodationId, string fileName) => $"{propertyId}/{accommodationId}/{fileName}";

        private static string ConstructPath(Guid propertyId, string fileName) => $"{propertyId}/{fileName}";
    }
}

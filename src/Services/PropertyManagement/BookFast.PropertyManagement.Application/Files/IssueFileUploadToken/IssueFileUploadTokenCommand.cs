using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.Files.IssueFileUploadToken
{
    public sealed record IssueFileUploadTokenCommand(Guid PropertyId, Guid? AccommodationId, string OriginalFileName) : ICommand<FileAccessToken>;
}

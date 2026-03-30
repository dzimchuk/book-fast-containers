namespace BookFast.PropertyManagement.Application.Files
{
    public interface IFileTokenIssuer
    {
        FileAccessToken IssueUploadToken(string blobPath, TimeSpan validity);
    }
}

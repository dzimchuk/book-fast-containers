namespace BookFast.Common.Application.Security
{
    public interface ISecurityContext
    {
        string GetCurrentUser();
        string GetCurrentTenant();
    }
}

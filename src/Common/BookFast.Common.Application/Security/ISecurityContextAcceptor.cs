using System.Security.Claims;

namespace BookFast.Common.Application.Security
{
    public interface ISecurityContextAcceptor
    {
        void SetPrincipal(ClaimsPrincipal value);
    }
}

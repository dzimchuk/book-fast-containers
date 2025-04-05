using BookFast.Common.Application.Security;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using ClaimTypes = BookFast.Common.Application.Security.ClaimTypes;

namespace BookFast.Common.Api.SecurityContext
{
    internal class SecurityContextProvider : ISecurityContext, ISecurityContextAcceptor
    {
        private ClaimsPrincipal principal;

        public string GetCurrentUser()
        {
            return FindFirstValue(Claims.Subject);
        }

        public string GetCurrentTenant()
        {
            return FindFirstValue(ClaimTypes.TenantId);
        }

        private string FindFirstValue(string claimType)
        {
            if (principal == null)
            {
                throw new Exception("Principal has not been initialized.");
            }

            var claim = principal.FindFirst(claimType);
            if (claim == null)
            {
                throw new Exception($"Claim '{claimType}' was not found.");
            }

            return claim.Value;
        }

        public void SetPrincipal(ClaimsPrincipal principal)
        {
            this.principal = principal;
        }
    }
}

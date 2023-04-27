using BookFast.Security;
using System;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace BookFast.Api.SecurityContext
{
    internal class SecurityContextProvider : ISecurityContext, ISecurityContextAcceptor
    {
        public ClaimsPrincipal Principal { get; set; }

        public string GetCurrentUser()
        {
            return FindFirstValue(Claims.Subject);
        }

        public string GetCurrentTenant()
        {
            return FindFirstValue(BookFastClaimTypes.TenantId);
        }

        private string FindFirstValue(string claimType)
        {
            if (Principal == null)
            {
                throw new Exception("Principal has not been initialized.");
            }

            var claim = Principal.FindFirst(claimType);
            if (claim == null)
            {
                throw new Exception($"Claim '{claimType}' was not found.");
            }

            return claim.Value;
        }
    }

}
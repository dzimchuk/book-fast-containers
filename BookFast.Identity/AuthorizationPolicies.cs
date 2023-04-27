using BookFast.Security;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace BookFast.Identity
{
    internal static class AuthorizationPolicies
    {
        public const string TenantAdmin = "TenantAdmin";

        public static void Register(AuthorizationOptions options, string apiAuthenticationScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
        {
            options.AddPolicy(TenantAdmin, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.TenantAdmin);
            });
        }
    }
}

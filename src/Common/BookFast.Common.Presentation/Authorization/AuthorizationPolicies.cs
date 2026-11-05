using BookFast.Common.Application.Security;
using Microsoft.AspNetCore.Authorization;

namespace BookFast.Common.Presentation.Authorization
{
    public static class AuthorizationPolicies
    {
        public const string GlobalAdmin = "GlobalAdmin";

        public const string TenantAdmin = "TenantAdmin";
        public const string TenantUser = "TenantUser";

        public const string TenantAdminOrUser = "TenantAdminOrUser";
                
        public const string Customer = "Customer";

        public static void Register(AuthorizationOptions options, string apiAuthenticationScheme)
        {
            options.AddPolicy(TenantAdmin, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.TenantAdmin);
            });

            options.AddPolicy(TenantUser, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.TenantUser);
            });

            options.AddPolicy(TenantAdminOrUser, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.TenantAdmin, Roles.TenantUser);
            });

            options.AddPolicy(GlobalAdmin, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.GlobalAdmin);
            });

            options.AddPolicy(Customer, policy =>
            {
                policy.AddAuthenticationSchemes(apiAuthenticationScheme);
                policy.RequireRole(Roles.Customer);
            });
        }
    }
}

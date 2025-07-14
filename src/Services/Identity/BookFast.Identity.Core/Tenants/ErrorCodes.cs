using BookFast.Common.SeedWork;

namespace BookFast.Identity.Core.Tenants
{
    internal static class ErrorCodes
    {
        public static Error TenantNotFound =>
            Error.NotFound("Tenant.TenantNotFound", "Tenant not found.");
    }
}

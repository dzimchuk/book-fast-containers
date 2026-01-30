using BookFast.Common.SeedWork;

namespace BookFast.Identity.Core.Tenants
{
    internal static class ErrorCodes
    {
        public static Error TenantNotFound =>
            Error.NotFound("Tenants.TenantNotFound", "Tenant not found.");

        public static Error TenantAlreadyExists(string name) =>
            Error.Problem("Tenants.Duplicate", $"Tenant '{name}' already exists.");
    }
}

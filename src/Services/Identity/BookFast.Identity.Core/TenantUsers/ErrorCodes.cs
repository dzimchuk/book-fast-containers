using BookFast.Common.SeedWork;

namespace BookFast.Identity.Core.TenantUsers
{
    internal static class ErrorCodes
    {
        public static Error UnsupportedRole =>
            Error.Problem("TenantUsers.UnsupportedRole", "Unsupported role.");

        public static Error UserNotFound =>
            Error.NotFound("TenantUsers.UserNotFound", "User not found.");

        public static Error SelfRoleChange =>
            Error.Problem("TenantUsers.SelfRoleChange", "User cannot change her own role.");

        public static Error SelfRemoval =>
            Error.Problem("TenantUsers.SelfRemoval", "User cannot remove herself.");
    }
}

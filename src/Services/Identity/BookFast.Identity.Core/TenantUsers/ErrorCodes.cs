using BookFast.Common.SeedWork;

namespace BookFast.Identity.Core.TenantUsers
{
    public static class ErrorCodes
    {
        public static Error UnsupportedRole =>
            Error.Problem("TenantUser.UnsupportedRole", "Unsupported role.");

        public static Error UserNotFound =>
            Error.NotFound("TenantUser.UserNotFound", "User not found.");

        public static Error SelfRoleChange =>
            Error.Problem("TenantUser.SelfRoleChange", "User cannot change her own role.");

        public static Error SelfRemoval =>
            Error.Problem("TenantUser.SelfRemoval", "User cannot remove herself.");
    }
}

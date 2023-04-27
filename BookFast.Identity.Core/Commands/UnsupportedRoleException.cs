namespace BookFast.Identity.Core.Commands
{
    public class UnsupportedRoleException : BusinessException
    {
        public UnsupportedRoleException()
            : base(ErrorCodes.UnsupportedRole, $"Specified role not allowed. Supported roles: {Roles.TenantUser}, {Roles.TenantAdmin}.")
        {
        }
    }
}

using BookFast.Common.Application.Queries;

namespace BookFast.Identity.Core.TenantUsers.ListTenantUsers
{
    public class ListTenantUsersQuery : ListQuery<TenantUserRepresentation>
    {
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}

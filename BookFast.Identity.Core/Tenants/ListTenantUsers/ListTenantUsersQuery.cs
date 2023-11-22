﻿using BookFast.SeedWork.Queries;

namespace BookFast.Identity.Core.Tenants.ListTenantUsers
{
    public class ListTenantUsersQuery : ListQuery<TenantUserRepresentation>
    {
        public string UserName { get; set; }
        public string Role { get; set; }
    }
}
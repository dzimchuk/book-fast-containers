using BookFast.SeedWork.Core.Queries;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.Identity.Core.Queries.ListTenantUsers
{
    public class ListTenantUsersHandler : ListQueryHandler<ListTenantUsersQuery, TenantUserRepresentation>
    {
        private const string userIdFieldName = nameof(TenantUserRepresentation.UserId);
        private const string userNameFieldName = nameof(TenantUserRepresentation.UserName);
        private const string roleFieldName = nameof(TenantUserRepresentation.Role);

        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public ListTenantUsersHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        protected override IQueryable<TenantUserRepresentation> FilterAndProject(ListTenantUsersQuery request)
        {
            var query = from user in dbContext.Users.AsNoTracking()
                        where user.TenantId == securityContext.GetCurrentTenant()
                        join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                        join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                        select new TenantUserRepresentation(user.Id, user.UserName, role.Name);

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var userName = request.UserName.ToLowerInvariant();
                query = query.Where(i => i.UserName.ToLower().Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var role = request.Role.ToLowerInvariant();
                query = query.Where(i => i.Role.ToLower().Contains(role));
            }

            return query;
        }

        protected override Dictionary<string, Expression<Func<TenantUserRepresentation, object>>> GetOrderingExpressionMap() => new()
        {
            { userIdFieldName, item => item.UserId },
            { userNameFieldName, item => item.UserName },
            { roleFieldName, item => item.Role }
        };

        protected override string GetDefaultOrderField() => userIdFieldName;
    }
}

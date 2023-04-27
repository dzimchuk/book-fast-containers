using Microsoft.EntityFrameworkCore;

namespace BookFast.Identity.Core.Queries.FindTenantUser
{
    public class FindTenantUserHandler : IRequestHandler<FindTenantUserQuery, TenantUserRepresentation>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public FindTenantUserHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<TenantUserRepresentation> Handle(FindTenantUserQuery request, CancellationToken cancellationToken)
        {
            var tenantUser = await (from user in dbContext.Users.AsNoTracking()
                                    where user.Id == request.Id && user.TenantId == securityContext.GetCurrentTenant()
                                    join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                                    join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                                    select new TenantUserRepresentation(user.Id, user.UserName, role.Name)).FirstOrDefaultAsync(cancellationToken);


            if (tenantUser == null)
            {
                throw new NotFoundException($"User {request.Id} was not found in tenant {securityContext.GetCurrentTenant()}.");
            }

            return tenantUser;
        }
    }
}

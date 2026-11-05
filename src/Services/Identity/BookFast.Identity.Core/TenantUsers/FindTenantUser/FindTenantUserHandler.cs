using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Identity.Core.TenantUsers.FindTenantUser
{
    public class FindTenantUserHandler : IQueryHandler<FindTenantUserQuery, TenantUserRepresentation>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public FindTenantUserHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result<TenantUserRepresentation>> Handle(FindTenantUserQuery request, CancellationToken cancellationToken)
        {
            var tenantUser = await (from user in dbContext.Users.AsNoTracking()
                                    where user.Id == request.UserId && user.TenantId == securityContext.GetCurrentTenant()
                                    join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
                                    join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                                    select new TenantUserRepresentation
                                    {
                                        UserId = user.Id,
                                        UserName = user.UserName,
                                        Role = role.Name
                                    }).FirstOrDefaultAsync(cancellationToken);


            if (tenantUser == null)
            {
                return ErrorCodes.UserNotFound;
            }

            return tenantUser;
        }
    }
}

using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Identity.Core.Tenants.FindTenant
{
    internal sealed class FindTenantQueryHandler(IDbContext dbContext) : IQueryHandler<FindTenantQuery, TenantRepresentation>
    {
        public async Task<Result<TenantRepresentation>> Handle(FindTenantQuery request, CancellationToken cancellationToken)
        {
            var result = await (from tenant in dbContext.Tenants.AsNoTracking()
                                where tenant.Id == request.TenantId
                                select new TenantRepresentation
                                {
                                    TenantId = tenant.Id,
                                    TenantName = tenant.Name
                                }).FirstOrDefaultAsync(cancellationToken);


            if (result == null)
            {
                return ErrorCodes.TenantNotFound;
            }

            return result;
        }
    }
}

using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty
{
    public class DeletePropertyHandler : ICommandHandler<DeletePropertyCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public DeletePropertyHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            var property = await dbContext.Properties.FirstOrDefaultAsync(
                p => p.Id == request.PropertyId && p.TenantId == tenantId,
                cancellationToken);

            if (property == null)
            {
                return ErrorCodes.PropertyNotFound(request.PropertyId);
            }

            if (await dbContext.Accommodations.AnyAsync(
                accommodation => accommodation.PropertyId == request.PropertyId && accommodation.TenantId == tenantId,
                cancellationToken: cancellationToken))
            {
                return ErrorCodes.PropertyNotEmpty(request.PropertyId);
            }

            property.Deactivate();

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

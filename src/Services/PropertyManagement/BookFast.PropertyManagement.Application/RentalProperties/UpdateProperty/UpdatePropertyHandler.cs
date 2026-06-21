using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty
{
    public class UpdatePropertyHandler : ICommandHandler<UpdatePropertyCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public UpdatePropertyHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties.FirstOrDefaultAsync(
                p => p.Id == request.PropertyId && p.TenantId == securityContext.GetCurrentTenant(),
                cancellationToken);
            if (property == null)
            {
                return ErrorCodes.PropertyNotFound(request.PropertyId);
            }

            property.Update(
                request.Name,
                request.Description,
                request.Address,
                request.Location,
                request.Images);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

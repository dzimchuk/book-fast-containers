using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.DeleteAccommodation
{
    public class DeleteAccommodationHandler : ICommandHandler<DeleteAccommodationCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public DeleteAccommodationHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(DeleteAccommodationCommand request, CancellationToken cancellationToken)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(
                    a => a.Id == request.AccommodationId && a.TenantId == securityContext.GetCurrentTenant(),
                    ct);
                if (accommodation == null)
                {
                    return ErrorCodes.AccommodationNotFound(request.AccommodationId);
                }

                dbContext.Accommodations.Remove(accommodation);

                await dbContext.SaveChangesAsync(ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}

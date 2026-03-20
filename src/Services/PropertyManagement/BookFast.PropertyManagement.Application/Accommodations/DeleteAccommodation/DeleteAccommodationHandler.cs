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
            var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(
                a => a.Id == request.AccommodationId && a.TenantId == securityContext.GetCurrentTenant(),
                cancellationToken);
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.AccommodationId);
            }

            dbContext.Accommodations.Remove(accommodation);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

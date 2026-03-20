using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationHandler : ICommandHandler<UpdateAccommodationCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public UpdateAccommodationHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result> Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await dbContext.Accommodations.FirstOrDefaultAsync(
                a => a.Id == request.AccommodationId && a.TenantId == securityContext.GetCurrentTenant(),
                cancellationToken);
            if (accommodation == null)
            {
                return ErrorCodes.AccommodationNotFound(request.AccommodationId);
            }

            accommodation.Update(
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images,
                request.Quantity,
                request.Price);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

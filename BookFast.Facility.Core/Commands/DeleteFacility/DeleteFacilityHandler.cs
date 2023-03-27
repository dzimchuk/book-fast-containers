using BookFast.SeedWork;

namespace BookFast.Facility.Core.Commands.DeleteFacility
{
    public class DeleteFacilityHandler : IRequestHandler<DeleteFacilityCommand>
    {
        private readonly IDbContext dbContext;

        public DeleteFacilityHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
        {
            var facility = await dbContext.Facilities.FindAsync(request.FacilityId);
            if (facility == null)
            {
                throw new NotFoundException("Facility", request.FacilityId);
            }

            if (await dbContext.Accommodations.AnyAsync(accommodation => accommodation.FacilityId == request.FacilityId, cancellationToken: cancellationToken))
            {
                throw new BusinessException(ErrorCodes.FacilityNotEmpty, "Facility cannot be deleted as it contains accommodations.");
            }

            dbContext.Facilities.Remove(facility);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

namespace BookFast.Facility.Core.Commands.UpdateFacility
{
    public class UpdateFacilityHandler : IRequestHandler<UpdateFacilityCommand>
    {
        private readonly IDbContext dbContext;

        public UpdateFacilityHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
        {
            var facility = await dbContext.Facilities.FindAsync(request.FacilityId);
            if (facility == null)
            {
                throw new NotFoundException("Facility", request.FacilityId);
            }

            facility.Update(
                request.Name,
                request.Description,
                request.StreetAddress,
                request.Latitude,
                request.Longitude,
                request.Images);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

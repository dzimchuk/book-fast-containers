using BookFast.Security;

namespace BookFast.Facility.Core.Commands.CreateFacility
{
    public class CreateFacilityHandler : IRequestHandler<CreateFacilityCommand, int>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public CreateFacilityHandler(IDbContext dbContext,
                                     ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<int> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
        {
            var facility = Models.Facility.NewFacility(
                securityContext.GetCurrentTenant(),
                request.Name,
                request.Description,
                request.StreetAddress,
                request.Latitude,
                request.Longitude,
                request.Images);

            await dbContext.Facilities.AddAsync(facility, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return facility.Id;
        }
    }
}

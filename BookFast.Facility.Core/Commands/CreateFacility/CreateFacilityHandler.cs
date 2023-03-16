using BookFast.Security;
using MediatR;

namespace BookFast.Facility.Core.Commands.CreateFacility
{
    public class CreateFacilityHandler : IRequestHandler<CreateFacilityCommand, int>
    {
        private readonly IRepository<Models.Facility, int> repository;
        private readonly ISecurityContext securityContext;

        public CreateFacilityHandler(IRepository<Models.Facility, int> repository,
                                     ISecurityContext securityContext)
        {
            this.repository = repository;
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

            var facilityId = await repository.AddAsync(facility);

            await repository.SaveChangesAsync();

            return facilityId;
        }
    }
}

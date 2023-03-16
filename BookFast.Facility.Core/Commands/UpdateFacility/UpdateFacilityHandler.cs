using MediatR;

namespace BookFast.Facility.Core.Commands.UpdateFacility
{
    public class UpdateFacilityHandler : IRequestHandler<UpdateFacilityCommand>
    {
        private readonly IRepository<Models.Facility, int> repository;

        public UpdateFacilityHandler(IRepository<Models.Facility, int> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
        {
            var facility = await repository.FindAsync(request.FacilityId);
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

            await repository.SaveChangesAsync();
        }
    }
}

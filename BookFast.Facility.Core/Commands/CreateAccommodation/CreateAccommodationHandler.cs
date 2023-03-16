using BookFast.Facility.Core.Models;
using MediatR;

namespace BookFast.Facility.Core.Commands.CreateAccommodation
{
    public class CreateAccommodationHandler : IRequestHandler<CreateAccommodationCommand, int>
    {
        private readonly IRepository<Accommodation, int> accommodationRepository;
        private readonly IRepository<Models.Facility, int> facilityRepository;

        public CreateAccommodationHandler(IRepository<Accommodation, int> accommodationRepository,
                                          IRepository<Models.Facility, int> facilityRepository)
        {
            this.accommodationRepository = accommodationRepository;
            this.facilityRepository = facilityRepository;
        }

        public async Task<int> Handle(CreateAccommodationCommand request, CancellationToken cancellationToken)
        {
            if (!await facilityRepository.AnyAsync(request.FacilityId))
            {
                throw new BusinessException("Facility not found");
            }


            var accommodation = Accommodation.NewAccommodation(
                request.FacilityId,
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images);

            accommodation.Id = await accommodationRepository.AddAsync(accommodation);

            await accommodationRepository.SaveChangesAsync();

            return accommodation.Id;
        }
    }
}

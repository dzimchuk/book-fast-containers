using BookFast.Facility.Core.Models;
using MediatR;

namespace BookFast.Facility.Core.Commands.UpdateAccommodation
{
    public class UpdateAccommodationHandler : IRequestHandler<UpdateAccommodationCommand>
    {
        private readonly IRepository<Accommodation, int> repository;

        public UpdateAccommodationHandler(IRepository<Accommodation, int> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await repository.FindAsync(request.AccommodationId);
            if (accommodation == null)
            {
                throw new NotFoundException("Accommodation", request.AccommodationId);
            }

            accommodation.Update(
                request.Name,
                request.Description,
                request.RoomCount,
                request.Images);

            await repository.SaveChangesAsync();
        }
    }
}

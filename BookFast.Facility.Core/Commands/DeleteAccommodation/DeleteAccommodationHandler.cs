using BookFast.Facility.Core.Models;
using MediatR;

namespace BookFast.Facility.Core.Commands.DeleteAccommodation
{
    public class DeleteAccommodationHandler : IRequestHandler<DeleteAccommodationCommand>
    {
        private readonly IRepository<Accommodation, int> repository;

        public DeleteAccommodationHandler(IRepository<Accommodation, int> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(DeleteAccommodationCommand request, CancellationToken cancellationToken)
        {
            var accommodation = await repository.FindAsync(request.AccommodationId);
            if (accommodation == null)
            {
                throw new NotFoundException("Accommodation", request.AccommodationId);
            }

            repository.Delete(accommodation.Id);

            await repository.SaveChangesAsync();
        }
    }
}

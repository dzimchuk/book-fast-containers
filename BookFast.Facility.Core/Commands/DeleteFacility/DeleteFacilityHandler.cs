using MediatR;

namespace BookFast.Facility.Core.Commands.DeleteFacility
{
    public class DeleteFacilityHandler : IRequestHandler<DeleteFacilityCommand>
    {
        private readonly IRepository<Models.Facility, int> repository;

        public DeleteFacilityHandler(IRepository<Models.Facility, int> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
        {
            var facility = await repository.FindAsync(request.FacilityId);
            if (facility == null)
            {
                throw new NotFoundException("Facility", request.FacilityId);
            }

            repository.Delete(facility.Id);

            await repository.SaveChangesAsync();
        }
    }
}

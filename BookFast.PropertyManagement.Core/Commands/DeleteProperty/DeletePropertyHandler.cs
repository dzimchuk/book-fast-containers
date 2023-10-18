using BookFast.SeedWork;

namespace BookFast.PropertyManagement.Core.Commands.DeleteFacility
{
    public class DeletePropertyHandler : IRequestHandler<DeletePropertyCommand>
    {
        private readonly IDbContext dbContext;

        public DeletePropertyHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties.FindAsync(request.PropertyId);
            if (property == null)
            {
                throw new NotFoundException("Property", request.PropertyId);
            }

            if (await dbContext.Accommodations.AnyAsync(accommodation => accommodation.PropertyId == request.PropertyId, cancellationToken: cancellationToken))
            {
                throw new BusinessException(ErrorCodes.PropertyNotEmpty, "Property cannot be deleted as it contains accommodations.");
            }

            property.Deactivate();

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

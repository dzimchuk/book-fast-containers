namespace BookFast.PropertyManagement.Core.Commands.UpdateFacility
{
    public class UpdatePropertyHandler : IRequestHandler<UpdatePropertyCommand>
    {
        private readonly IDbContext dbContext;

        public UpdatePropertyHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties.FindAsync(request.PropertyId);
            if (property == null)
            {
                throw new NotFoundException("Property", request.PropertyId);
            }

            property.Update(
                request.Name,
                request.Description,
                request.Address,
                request.Location,
                request.Images);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

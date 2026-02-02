using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;

namespace BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty
{
    public class UpdatePropertyHandler : ICommandHandler<UpdatePropertyCommand>
    {
        private readonly IDbContext dbContext;

        public UpdatePropertyHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties.FindAsync(request.PropertyId);
            if (property == null)
            {
                return ErrorCodes.PropertyNotFound(request.PropertyId);
            }

            property.Update(
                request.Name,
                request.Description,
                request.Address,
                request.Location,
                request.Images);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

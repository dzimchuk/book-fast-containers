using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty
{
    public class DeletePropertyHandler : ICommandHandler<DeletePropertyCommand>
    {
        private readonly IDbContext dbContext;

        public DeletePropertyHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await dbContext.Properties.FindAsync(request.PropertyId);
            if (property == null)
            {
                return ErrorCodes.PropertyNotFound(request.PropertyId);
            }

            if (await dbContext.Accommodations.AnyAsync(accommodation => accommodation.PropertyId == request.PropertyId, cancellationToken: cancellationToken))
            {
                return ErrorCodes.PropertyNotEmpty(request.PropertyId);
            }

            property.Deactivate();

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

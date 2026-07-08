using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.RentalProperties.CreateProperty
{
    public class CreatePropertyHandler : ICommandHandler<CreatePropertyCommand, Guid>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public CreatePropertyHandler(IDbContext dbContext,
                                     ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result<Guid>> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = Property.NewProperty(
                securityContext.GetCurrentTenant(),
                request.Name,
                request.Description,
                request.Address,
                request.Location,
                request.Images,
                request.Facilities);

            property.Id = Guid.CreateVersion7();

            await dbContext.Properties.AddAsync(property, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return property.Id;
        }
    }
}

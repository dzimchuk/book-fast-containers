using BookFast.Security;

namespace BookFast.PropertyManagement.Core.Commands.CreateFacility
{
    public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, int>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public CreatePropertyHandler(IDbContext dbContext,
                                     ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<int> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = Property.NewProperty(
                securityContext.GetCurrentTenant(),
                request.Name,
                request.Description,
                request.Address,
                request.Location,
                request.Images);

            await dbContext.Properties.AddAsync(property, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return property.Id;
        }
    }
}

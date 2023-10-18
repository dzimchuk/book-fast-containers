using BookFast.PropertyManagement.Core.Queries.GetProperty;
using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.GetFacility
{
    public class GetPropertyHandler : IRequestHandler<GetPropertyQuery, PropertyRepresentation>
    {
        private readonly IDbContext dbContext;

        public GetPropertyHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<PropertyRepresentation> Handle(GetPropertyQuery request, CancellationToken cancellationToken)
        {
            var property = await (from item in dbContext.Properties.AsNoTracking()
                                  where item.Id == request.Id
                                  select new
                                  {
                                      item.Id,
                                      item.Name,
                                      item.Description,
                                      item.Address,
                                      item.Location,
                                      item.Images,
                                      item.IsActive
                                  }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

            return property == null
                ? throw new NotFoundException("Property", request.Id)
                : new PropertyRepresentation
                {
                    Id = property.Id,
                    Name = property.Name,
                    Description = property.Description,
                    Address = AddressRepresentation.Map(property.Address),
                    Location = LocationRepresentation.Map(property.Location),
                    Images = property.Images,
                    IsActive = property.IsActive
                };
        }
    }
}

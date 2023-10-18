using BookFast.PropertyManagement.Core.Queries.Representations;
using BookFast.Security;

namespace BookFast.PropertyManagement.Core.Queries.ListFacilities
{
    public class ListPropertiesHandler : IRequestHandler<ListPropertiesQuery, IEnumerable<PropertyRepresentation>>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public ListPropertiesHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<IEnumerable<PropertyRepresentation>> Handle(ListPropertiesQuery request, CancellationToken cancellationToken)
        {
            var properties = await (from item in dbContext.Properties.AsNoTracking()
                                    where item.Owner == securityContext.GetCurrentTenant()
                                    select new
                                    {
                                        item.Id,
                                        item.Name,
                                        item.Description,
                                        item.Address,
                                        item.Location,
                                        item.Images,
                                        item.IsActive
                                    }).ToListAsync(cancellationToken: cancellationToken);

            return (from property in properties
                    select new PropertyRepresentation
                    {
                        Id = property.Id,
                        Name = property.Name,
                        Description = property.Description,
                        Address = AddressRepresentation.Map(property.Address),
                        Location = LocationRepresentation.Map(property.Location),
                        Images = property.Images,
                        IsActive = property.IsActive
                    }).ToList();
        }
    }
}

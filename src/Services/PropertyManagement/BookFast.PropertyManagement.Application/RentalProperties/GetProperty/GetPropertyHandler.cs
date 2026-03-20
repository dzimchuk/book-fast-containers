using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.GetProperty
{
    public class GetPropertyHandler : IQueryHandler<GetPropertyQuery, PropertyRepresentation>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public GetPropertyHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        public async Task<Result<PropertyRepresentation>> Handle(GetPropertyQuery request, CancellationToken cancellationToken)
        {
            var property = await (from item in dbContext.Properties.AsNoTracking()
                                  where item.Id == request.Id && item.TenantId == securityContext.GetCurrentTenant()
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

            if (property == null)
            {
                return ErrorCodes.PropertyNotFound(request.Id);
            }

            return new PropertyRepresentation
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

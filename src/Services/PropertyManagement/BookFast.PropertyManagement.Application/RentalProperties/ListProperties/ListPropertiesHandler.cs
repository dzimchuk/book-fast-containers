using BookFast.Common.Application.Queries;
using BookFast.Common.Application.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.PropertyManagement.Application.RentalProperties.ListProperties
{
    public class ListPropertiesHandler : ListQueryHandler<ListPropertiesQuery, PropertyRepresentation>
    {
        private const string idFieldName = nameof(PropertyRepresentation.Id);
        private const string nameFieldName = nameof(PropertyRepresentation.Name);
        private const string isActiveFieldName = nameof(PropertyRepresentation.IsActive);

        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public ListPropertiesHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        protected override IQueryable<PropertyRepresentation> FilterAndProject(ListPropertiesQuery request)
        {
            var query = from item in dbContext.Properties.AsNoTracking()
                        where item.TenantId == securityContext.GetCurrentTenant()
                        select new PropertyRepresentation
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Description = item.Description,
                            Address = new AddressRepresentation
                            {
                                Country = item.Address.Country,
                                State = item.Address.State,
                                City = item.Address.City,
                                Street = item.Address.Street,
                                ZipCode = item.Address.ZipCode
                            },
                            Location = new LocationRepresentation
                            {
                                Latitude = item.Location.Latitude,
                                Longitude = item.Location.Longitude
                            },
                            Images = item.Images,
                            Facilities = item.Facilities,
                            IsActive = item.IsActive
                        };

            return query;
        }

        protected override string GetDefaultOrderField() => nameFieldName;

        protected override Dictionary<string, Expression<Func<PropertyRepresentation, object>>> GetOrderingExpressionMap() => new()
        {
            { idFieldName, item => item.Id },
            { nameFieldName, item => item.Name },
            { isActiveFieldName, item => item.IsActive }
        };
    }
}

using BookFast.Common.Application.Queries;
using BookFast.Common.Application.Security;
using BookFast.PropertyManagement.Application.RentalProperties;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.PropertyManagement.Application.Accommodations.ListAccommodations
{
    public class ListAccommodationsHandler : ListQueryHandler<ListAccommodationsQuery, AccommodationRepresentation>
    {
        private const string idFieldName = nameof(PropertyRepresentation.Id);
        private const string nameFieldName = nameof(PropertyRepresentation.Name);
        private const string isActiveFieldName = nameof(PropertyRepresentation.IsActive);

        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;

        public ListAccommodationsHandler(IDbContext dbContext, ISecurityContext securityContext)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
        }

        protected override IQueryable<AccommodationRepresentation> FilterAndProject(ListAccommodationsQuery request)
        {
            var query = from item in dbContext.Accommodations.AsNoTracking()
                        where item.PropertyId == request.PropertyId && item.TenantId == securityContext.GetCurrentTenant()
                        select new AccommodationRepresentation
                        {
                            Id = item.Id,
                            PropertyId = item.PropertyId,
                            Name = item.Name,
                            Description = item.Description,
                            Bedrooms = item.Bedrooms,
                            Images = item.Images,
                            Quantity = item.Quantity,
                            PriceRange = item.PriceRange.MinPrice == null && item.PriceRange.MaxPrice == null ? null : new PriceRangeRepresentation
                            {
                                MinPrice = item.PriceRange.MinPrice == null ? null : new MoneyRepresentation
                                {
                                    Amount = item.PriceRange.MinPrice.Amount,
                                    Currency = item.PriceRange.MinPrice.Currency
                                },
                                MaxPrice = item.PriceRange.MaxPrice == null ? null : new MoneyRepresentation
                                {
                                    Amount = item.PriceRange.MaxPrice.Amount,
                                    Currency = item.PriceRange.MaxPrice.Currency
                                }
                            },
                            Facilities = item.Facilities,
                            IsActive = item.IsActive
                        };

            return query;
        }

        protected override string GetDefaultOrderField() => nameFieldName;

        protected override Dictionary<string, Expression<Func<AccommodationRepresentation, object>>> GetOrderingExpressionMap() => new()
        {
            { idFieldName, item => item.Id },
            { nameFieldName, item => item.Name },
            { isActiveFieldName, item => item.IsActive }
        };
    }
}

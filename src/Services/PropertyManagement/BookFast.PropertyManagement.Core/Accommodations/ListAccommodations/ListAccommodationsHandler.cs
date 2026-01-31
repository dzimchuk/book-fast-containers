using BookFast.Common.Application.Queries;
using BookFast.PropertyManagement.Core.Accommodations;
using BookFast.PropertyManagement.Core.RentalProperties;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BookFast.PropertyManagement.Core.Accommodations.ListAccommodations
{
    public class ListAccommodationsHandler : ListQueryHandler<ListAccommodationsQuery, AccommodationRepresentation>
    {
        private const string idFieldName = nameof(PropertyRepresentation.Id);
        private const string nameFieldName = nameof(PropertyRepresentation.Name);
        private const string isActiveFieldName = nameof(PropertyRepresentation.IsActive);

        private readonly IDbContext dbContext;

        public ListAccommodationsHandler(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override IQueryable<AccommodationRepresentation> FilterAndProject(ListAccommodationsQuery request)
        {
            var query = from item in dbContext.Accommodations.AsNoTracking().Where(item => item.PropertyId == request.PropertyId)
                        where item.PropertyId == request.PropertyId
                        select new AccommodationRepresentation
                        {
                            Id = item.Id,
                            PropertyId = item.PropertyId,
                            Name = item.Name,
                            Description = item.Description,
                            RoomCount = item.RoomCount,
                            Images = item.Images,
                            Quantity = item.Quantity,
                            Price = item.Price,
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

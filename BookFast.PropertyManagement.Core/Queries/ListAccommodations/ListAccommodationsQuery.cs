using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.ListAccommodations
{
    public class ListAccommodationsQuery : IRequest<IEnumerable<AccommodationRepresentation>>
    {
        public int PropertyId { get; set; }
    }
}

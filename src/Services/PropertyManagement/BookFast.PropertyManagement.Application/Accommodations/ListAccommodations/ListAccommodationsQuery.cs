using BookFast.Common.Application.Queries;

namespace BookFast.PropertyManagement.Application.Accommodations.ListAccommodations
{
    public class ListAccommodationsQuery : ListQuery<AccommodationRepresentation>
    {
        public int PropertyId { get; set; }
    }
}

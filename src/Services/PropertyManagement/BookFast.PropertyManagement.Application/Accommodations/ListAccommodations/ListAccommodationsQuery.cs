using BookFast.Common.Application.Queries;

namespace BookFast.PropertyManagement.Application.Accommodations.ListAccommodations
{
    public class ListAccommodationsQuery : ListQuery<AccommodationRepresentation>
    {
        public Guid PropertyId { get; set; }
    }
}

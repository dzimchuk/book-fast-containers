using BookFast.Common.Application.Queries;
using BookFast.PropertyManagement.Core.Accommodations;

namespace BookFast.PropertyManagement.Core.Accommodations.ListAccommodations
{
    public class ListAccommodationsQuery : ListQuery<AccommodationRepresentation>
    {
        public int PropertyId { get; set; }
    }
}

using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.ListAccommodations
{
    public class ListAccommodationsQuery : IRequest<IEnumerable<AccommodationRepresentation>>
    {
        public int FacilityId { get; set; }
    }
}

using BookFast.Facility.Core.Queries.Representations;

namespace BookFast.Facility.Core.Queries.GetAccommodation
{
    public class GetAccommodationQuery : IRequest<AccommodationRepresentation>
    {
        public int Id { get; set; }
    }
}

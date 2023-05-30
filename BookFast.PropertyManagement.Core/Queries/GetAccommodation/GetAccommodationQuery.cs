using BookFast.PropertyManagement.Core.Queries.Representations;

namespace BookFast.PropertyManagement.Core.Queries.GetAccommodation
{
    public class GetAccommodationQuery : IRequest<AccommodationRepresentation>
    {
        public int Id { get; set; }
    }
}

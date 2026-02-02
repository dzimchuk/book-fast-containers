using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.Accommodations.GetAccommodation
{
    public class GetAccommodationQuery : IQuery<AccommodationRepresentation>
    {
        public int Id { get; set; }
    }
}

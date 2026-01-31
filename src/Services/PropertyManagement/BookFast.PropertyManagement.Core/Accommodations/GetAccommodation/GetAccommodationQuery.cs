using BookFast.Common.Application.Messaging;
using BookFast.PropertyManagement.Core.Accommodations;

namespace BookFast.PropertyManagement.Core.Accommodations.GetAccommodation
{
    public class GetAccommodationQuery : IQuery<AccommodationRepresentation>
    {
        public int Id { get; set; }
    }
}

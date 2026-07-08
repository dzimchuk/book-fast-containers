using BookFast.Common.Application.Messaging;
using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationCommand : ICommand
    {
        [SwaggerIgnore]
        public Guid AccommodationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? Bedrooms { get; set; }

        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public PriceRange PriceRange { get; set; }
    }
}

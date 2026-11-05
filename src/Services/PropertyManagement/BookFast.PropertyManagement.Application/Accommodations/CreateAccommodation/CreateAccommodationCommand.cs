using BookFast.Common.Application.Messaging;
using BookFast.Common.Domain;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation
{
    public class CreateAccommodationCommand : ICommand<Guid>
    {
        [SwaggerIgnore]
        public Guid PropertyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? Bedrooms { get; set; }

        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public PriceRange PriceRange { get; set; }

        public Facility[] Facilities { get; set; }
    }
}

using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Core.Accommodations.UpdateAccommodation
{
    public class UpdateAccommodationCommand : ICommand
    {
        [SwaggerIgnore]
        public int AccommodationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int RoomCount { get; set; }

        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

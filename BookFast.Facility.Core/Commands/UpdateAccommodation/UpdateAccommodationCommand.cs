using MediatR;

namespace BookFast.Facility.Core.Commands.UpdateAccommodation
{
    public class UpdateAccommodationCommand : IRequest
    {
        [SwaggerIgnore]
        public int AccommodationId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int RoomCount { get; set; }

        public string[] Images { get; set; }
    }
}

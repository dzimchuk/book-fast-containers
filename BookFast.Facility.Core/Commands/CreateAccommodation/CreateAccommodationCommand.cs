namespace BookFast.Facility.Core.Commands.CreateAccommodation
{
    public class CreateAccommodationCommand : IRequest<int>
    {
        [SwaggerIgnore]
        public int FacilityId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int RoomCount { get; set; }

        public string[] Images { get; set; }
    }
}

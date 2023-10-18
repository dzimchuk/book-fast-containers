namespace BookFast.PropertyManagement.Core.Commands.CreateAccommodation
{
    public class CreateAccommodationCommand : IRequest<int>
    {
        [SwaggerIgnore]
        public int PropertyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int RoomCount { get; set; }

        public string[] Images { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

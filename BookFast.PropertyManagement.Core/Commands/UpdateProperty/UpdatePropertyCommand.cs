namespace BookFast.PropertyManagement.Core.Commands.UpdateFacility
{
    public class UpdatePropertyCommand : IRequest
    {
        [SwaggerIgnore]
        public int PropertyId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public Address Address { get; set; }
        public Location Location { get; set; }

        public string[] Images { get; set; }
    }
}

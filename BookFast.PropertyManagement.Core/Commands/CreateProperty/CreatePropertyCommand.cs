namespace BookFast.PropertyManagement.Core.Commands.CreateFacility
{
    public class CreatePropertyCommand : IRequest<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Address Address { get; set; }
        public Location Location { get; set; }

        public string[] Images { get; set; }
    }
}

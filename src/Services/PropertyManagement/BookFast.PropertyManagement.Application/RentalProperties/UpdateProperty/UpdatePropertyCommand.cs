using BookFast.Common.Application.Messaging;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty
{
    public class UpdatePropertyCommand : ICommand
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

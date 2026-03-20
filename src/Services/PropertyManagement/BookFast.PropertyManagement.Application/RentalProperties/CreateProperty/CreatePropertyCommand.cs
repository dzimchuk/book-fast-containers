using BookFast.Common.Application.Messaging;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.RentalProperties.CreateProperty
{
    public class CreatePropertyCommand : ICommand<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Address Address { get; set; }
        public Location Location { get; set; }

        public string[] Images { get; set; }
    }
}

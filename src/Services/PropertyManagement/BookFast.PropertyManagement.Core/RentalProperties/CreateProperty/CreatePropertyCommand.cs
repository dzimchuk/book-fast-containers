using BookFast.Common.Application.Messaging;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Core.RentalProperties.CreateProperty
{
    public class CreatePropertyCommand : ICommand<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Address Address { get; set; }
        public Location Location { get; set; }

        public string[] Images { get; set; }
    }
}

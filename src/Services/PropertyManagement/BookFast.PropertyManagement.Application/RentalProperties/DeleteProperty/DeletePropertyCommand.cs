using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty
{
    public class DeletePropertyCommand : ICommand
    {
        public int PropertyId { get; set; }
    }
}

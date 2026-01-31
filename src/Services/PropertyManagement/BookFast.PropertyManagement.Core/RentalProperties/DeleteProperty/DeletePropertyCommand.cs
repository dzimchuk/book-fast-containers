using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Core.RentalProperties.DeleteProperty
{
    public class DeletePropertyCommand : ICommand
    {
        public int PropertyId { get; set; }
    }
}

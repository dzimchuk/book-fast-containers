using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty
{
    public class DeletePropertyCommand : ICommand
    {
        public Guid PropertyId { get; set; }
    }
}

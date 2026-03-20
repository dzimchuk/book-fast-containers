using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Application.Accommodations.DeleteAccommodation
{
    public class DeleteAccommodationCommand : ICommand
    {
        public Guid AccommodationId { get; set; }
    }
}

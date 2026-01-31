using BookFast.Common.Application.Messaging;

namespace BookFast.PropertyManagement.Core.Accommodations.DeleteAccommodation
{
    public class DeleteAccommodationCommand : ICommand
    {
        public int AccommodationId { get; set; }
    }
}

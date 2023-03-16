using MediatR;

namespace BookFast.Facility.Core.Commands.DeleteAccommodation
{
    public class DeleteAccommodationCommand : IRequest
    {
        public int AccommodationId { get; set; }
    }
}

using BookFast.Booking.Application.Accommodations.MarkNotBookable;
using BookFast.PropertyManagement.Integration;
using MassTransit;
using MediatR;

namespace BookFast.Booking.Infrastructure.Consumers
{
    internal class AccommodationDeletedEventConsumer(ISender sender) : IConsumer<AccommodationDeletedEvent>
    {
        public Task Consume(ConsumeContext<AccommodationDeletedEvent> context)
        {
            var message = context.Message;

            var command = new MarkAccommodationNotBookableCommand
            {
                AccommodationId = message.AccommodationId,
                PropertyId = message.PropertyId,
                TenantId = message.TenantId,
                OccurredAt = message.OccurredAt
            };

            return sender.Send(command, context.CancellationToken);
        }
    }
}

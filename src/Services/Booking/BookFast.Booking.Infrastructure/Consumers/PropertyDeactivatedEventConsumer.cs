using BookFast.Booking.Application.Accommodations.MarkNotBookable;
using BookFast.PropertyManagement.Integration;
using MassTransit;
using MediatR;

namespace BookFast.Booking.Infrastructure.Consumers
{
    internal class PropertyDeactivatedEventConsumer(ISender sender) : IConsumer<PropertyDeactivatedEvent>
    {
        public Task Consume(ConsumeContext<PropertyDeactivatedEvent> context)
        {
            var message = context.Message;

            var command = new MarkPropertyNotBookableCommand
            {
                PropertyId = message.PropertyId,
                TenantId = message.TenantId,
                OccurredAt = message.OccurredAt
            };

            return sender.Send(command, context.CancellationToken);
        }
    }
}

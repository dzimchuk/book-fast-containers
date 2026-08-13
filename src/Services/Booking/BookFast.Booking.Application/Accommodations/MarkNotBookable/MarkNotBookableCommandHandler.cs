using BookFast.Booking.Domain;
using BookFast.Booking.Integration;
using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace BookFast.Booking.Application.Accommodations.MarkNotBookable
{
    public class MarkNotBookableCommandHandler(IDbContext dbContext, IIntegrationEventPublisher eventPublisher)
        : ICommandHandler<MarkAccommodationNotBookableCommand>,
          ICommandHandler<MarkPropertyNotBookableCommand>
    {
        public Task<Result> Handle(MarkAccommodationNotBookableCommand request, CancellationToken cancellationToken)
        {
            return dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodation = await dbContext.Accommodations.FindAsync([request.AccommodationId], ct);

                if (accommodation is null)
                {
                    // a delete racing an as-yet-unseen create still needs a row, so a later out-of-order create can't resurrect it
                    accommodation = Accommodation.NewAccommodation(request.AccommodationId, request.TenantId, request.PropertyId);
                    dbContext.Accommodations.Add(accommodation);
                }

                var changed = accommodation.MarkNotBookable(request.OccurredAt);

                await dbContext.SaveChangesAsync(ct);

                if (changed)
                {
                    await PublishBookableChangedAsync(accommodation, ct);
                }

                return Result.Success();
            }, cancellationToken);
        }

        public Task<Result> Handle(MarkPropertyNotBookableCommand request, CancellationToken cancellationToken)
        {
            return dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var accommodations = await dbContext.Accommodations.Where(a => a.PropertyId == request.PropertyId).ToListAsync(ct);

                var changedAccommodations = accommodations.Where(a => a.MarkNotBookable(request.OccurredAt)).ToList();

                await dbContext.SaveChangesAsync(ct);

                foreach (var accommodation in changedAccommodations)
                {
                    await PublishBookableChangedAsync(accommodation, ct);
                }

                return Result.Success();
            }, cancellationToken);
        }

        private Task PublishBookableChangedAsync(Accommodation accommodation, CancellationToken cancellationToken) =>
            eventPublisher.PublishAsync(new AccommodationBookableChanged
            {
                OccurredAt = accommodation.OccurredAt,
                AccommodationId = accommodation.Id,
                Bookable = accommodation.Bookable,
                Rate = accommodation.Rate,
            }, cancellationToken);
    }
}

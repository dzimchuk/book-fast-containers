using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;

namespace BookFast.PropertyManagement.Application.RentalProperties.DeleteProperty
{
    public class DeletePropertyHandler : ICommandHandler<DeletePropertyCommand>
    {
        private readonly IDbContext dbContext;
        private readonly ISecurityContext securityContext;
        private readonly IIntegrationEventPublisher eventPublisher;

        public DeletePropertyHandler(IDbContext dbContext,
                                     ISecurityContext securityContext,
                                     IIntegrationEventPublisher eventPublisher)
        {
            this.dbContext = dbContext;
            this.securityContext = securityContext;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
        {
            var tenantId = securityContext.GetCurrentTenant();

            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var property = await dbContext.Properties.FirstOrDefaultAsync(
                    p => p.Id == request.PropertyId && p.TenantId == tenantId,
                    ct);

                if (property == null)
                {
                    return ErrorCodes.PropertyNotFound(request.PropertyId);
                }

                if (await dbContext.Accommodations.AnyAsync(
                    accommodation => accommodation.PropertyId == request.PropertyId && accommodation.TenantId == tenantId,
                    cancellationToken: ct))
                {
                    return ErrorCodes.PropertyNotEmpty(request.PropertyId);
                }

                property.Deactivate();

                await dbContext.SaveChangesAsync(ct);

                await eventPublisher.PublishAsync(new PropertyDeactivatedEvent
                {
                    TenantId = tenantId,
                    PropertyId = property.Id
                }, ct);

                return Result.Success();
            }, cancellationToken);
        }
    }
}

using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace BookFast.Identity.Core.Tenants.AddTenant
{
    internal class AddTenantCommandHandler(IDbContext dbContext,
                                           UserManager<User> userManager,
                                           IUserStore<User> userStore,
                                           IEmailConfirmationSender confirmationSender)
        : ICommandHandler<AddTenantCommand, string>
    {
        public async Task<Result<string>> Handle(AddTenantCommand request, CancellationToken cancellationToken)
        {
            return await dbContext.ExecuteInTransactionAsync(async ct =>
            {
                var upperTenantName = request.Name?.ToUpperInvariant();
                if (dbContext.Tenants.Any(t => t.Name.ToUpper() == upperTenantName))
                {
                    return ErrorCodes.TenantAlreadyExists(request.Name);
                }

                var tenant = new Tenant
                {
                    Id = Guid.CreateVersion7().ToString(),
                    Name = request.Name
                };

                dbContext.Tenants.Add(tenant);

                await dbContext.SaveChangesAsync(ct);

                var user = new User
                {
                    Id = Guid.CreateVersion7().ToString(),
                    TenantId = tenant.Id
                };

                await userStore.SetUserNameAsync(user, request.TenantAdmin, ct);

                var emailStore = userStore as IUserEmailStore<User>;
                if (emailStore != null)
                {
                    await emailStore.SetEmailAsync(user, request.TenantAdmin, ct);
                }

                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                result = await userManager.AddToRoleAsync(user, Roles.TenantAdmin);
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                await confirmationSender.SendAsync(user);

                return tenant.Id;
            }, cancellationToken);
        }
    }
}

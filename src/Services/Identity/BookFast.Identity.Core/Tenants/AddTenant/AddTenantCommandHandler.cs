using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace BookFast.Identity.Core.Tenants.AddTenant
{
    internal class AddTenantCommandHandler(IDbContext dbContext,
                                           UserManager<User> userManager,
                                           IUserStore<User> userStore)
        : ICommandHandler<AddTenantCommand, string>
    {
        public async Task<Result<string>> Handle(AddTenantCommand request, CancellationToken cancellationToken)
        {
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                var tenant = new Tenant
                {
                    Id = Guid.CreateVersion7().ToString(),
                    Name = request.Name
                };

                dbContext.Tenants.Add(tenant);

                await dbContext.SaveChangesAsync(cancellationToken);

                var user = new User
                {
                    Id = Guid.CreateVersion7().ToString(),
                    TenantId = tenant.Id
                };

                await userStore.SetUserNameAsync(user, request.TenantAdmin, cancellationToken);

                var emailStore = userStore as IUserEmailStore<User>;
                if (emailStore != null)
                {
                    await emailStore.SetEmailAsync(user, request.TenantAdmin, cancellationToken);
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

                scope.Complete();

                return tenant.Id;
            }
        }
    }
}

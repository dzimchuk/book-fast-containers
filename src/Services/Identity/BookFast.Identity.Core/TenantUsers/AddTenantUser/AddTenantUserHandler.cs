using BookFast.Common.Application.Messaging;
using BookFast.Common.Application.Security;
using BookFast.Common.SeedWork;
using BookFast.Identity.Core.Models;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace BookFast.Identity.Core.TenantUsers.AddTenantUser
{
    public class AddTenantUserHandler : ICommandHandler<AddTenantUserCommand, string>
    {
        private readonly UserManager<User> userManager;
        private readonly IUserStore<User> userStore;
        private readonly RoleManager<Role> roleManager;
        private readonly ISecurityContext securityContext;

        public AddTenantUserHandler(UserManager<User> userManager,
                                    IUserStore<User> userStore,
                                    RoleManager<Role> roleManager,
                                    ISecurityContext securityContext)
        {
            this.userManager = userManager;
            this.userStore = userStore;
            this.roleManager = roleManager;
            this.securityContext = securityContext;
        }

        public async Task<Result<string>> Handle(AddTenantUserCommand request, CancellationToken cancellationToken)
        {
            if (await roleManager.FindByNameAsync(request.Role.ToLowerInvariant()) == null)
            {
                return Result.Failure<string>(ErrorCodes.UnsupportedRole);
            }

            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                var user = new User 
                {
                    Id = Guid.CreateVersion7().ToString(),
                    TenantId = securityContext.GetCurrentTenant()
                };

                await userStore.SetUserNameAsync(user, request.UserName, cancellationToken);

                var emailStore = userStore as IUserEmailStore<User>;
                if (emailStore != null)
                {
                    await emailStore.SetEmailAsync(user, request.UserName, cancellationToken);
                }

                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                result = await userManager.AddToRoleAsync(user, request.Role.ToLowerInvariant());
                if (!result.Succeeded)
                {
                    return Result.Failure<string>(new ErrorCollection([.. result.Errors.Select(e => Error.Problem(e.Code, e.Description))]));
                }

                scope.Complete();

                return user.Id;
            }
        }
    }
}

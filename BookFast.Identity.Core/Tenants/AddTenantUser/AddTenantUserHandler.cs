using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace BookFast.Identity.Core.Tenants.AddTenantUser
{
    public class AddTenantUserHandler : IRequestHandler<AddTenantUserCommand, string>
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

        public async Task<string> Handle(AddTenantUserCommand request, CancellationToken cancellationToken)
        {
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                    TransactionScopeAsyncFlowOption.Enabled))
            {
                if (await roleManager.FindByNameAsync(request.Role) == null)
                {
                    throw new UnsupportedRoleException();
                }

                var user = new User { TenantId = securityContext.GetCurrentTenant() };

                await userStore.SetUserNameAsync(user, request.UserName, CancellationToken.None);

                var emailStore = userStore as IUserEmailStore<User>;
                if (emailStore != null)
                {
                    await emailStore.SetEmailAsync(user, request.UserName, CancellationToken.None);
                }

                var result = await userManager.CreateAsync(user);

                result.ThrowIfNotSucceeded();

                result = await userManager.AddToRoleAsync(user, request.Role);

                result.ThrowIfNotSucceeded();

                scope.Complete();

                return user.Id;
            }
        }
    }
}

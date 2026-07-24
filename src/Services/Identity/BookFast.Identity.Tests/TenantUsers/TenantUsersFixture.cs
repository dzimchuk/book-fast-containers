using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BookFast.Identity.Tests.TenantUsers
{
    public class TenantUsersFixture : IAsyncLifetime
    {
        public const string TestTenant1Id = Constants.CallerTenant;
        public const string TestTenant1Name = "Test tenant 1";

        public const string TestTenant2Id = "8c4a4e73-bf2d-46ca-bbcc-0f018b17ebe0";
        public const string TestTenant2Name = "Test tenant 2";

        public const string Tenant1Admin = "a319107f-7e90-4a4d-a56a-8d7d4df13bab";
        public const string Tenant2Admin = "c21fc6ff-e04c-4896-bd3f-03e3ad0ede11";
        
        public const string Tenant1User = "eeed345a-d18e-4e8f-a40f-9aafbc0e1305";

        private const string TenantAdminRoleId = "01960677-b45b-778a-b8de-f5c7579c597d";
        private const string TenantUserRoleId = "01960677-b45b-7c7d-9203-199c657cc7ff";


        private readonly WebApplicationFactory<Program> factory;
        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly IdentityContext dbContext;

        public HttpClient HttpClient => httpClient;

        public IDbContext DbContext => dbContext;

        public TenantUsersFixture(IdentityApiFixture fixture)
        {
            factory = fixture.GetWebApplicationFactory(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Tenant1Admin,
                    Role = Roles.TenantAdmin,
                    TenantId = TestTenant1Id
                });

                services.AddSingleton(Mock.Of<IMailNotificationQueue>());
            });
            httpClient = factory.CreateClient();

            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        }

        public async Task InitializeAsync()
        {
            dbContext.Tenants.AddRange(
                [
                    new Tenant
                    {
                        Id = TestTenant1Id,
                        Name = TestTenant1Name
                    },
                    new Tenant
                    {
                        Id = TestTenant2Id,
                        Name = TestTenant2Name
                    }
                ]);

            dbContext.Users.AddRange(
                new User { Id = Tenant1Admin, Email = "admin@test1.com", UserName = "admin@test1.com", TenantId = TestTenant1Id, NormalizedUserName = "ADMIN@TEST1.COM" },
                new User { Id = Tenant2Admin, Email = "admin@test2.com", UserName = "admin@test2.com", TenantId = TestTenant2Id, NormalizedUserName = "ADMIN@TEST2.COM" },
                new User { Id = Tenant1User, Email = "user1@test1.com", UserName = "user1@test1.com", TenantId = TestTenant1Id, NormalizedUserName = "USER1@TEST1.COM", SecurityStamp = "test" },
                new User { Id = "dbb3eaec-20ac-49e1-8f82-c35ee195be25", Email = "user2@test1.com", UserName = "user2@test1.com", TenantId = TestTenant1Id, NormalizedUserName = "USER2@TEST1.COM" },
                new User { Id = "12899f34-7d73-42be-8cc6-2d503dbe40f6", Email = "test@test2.com", UserName = "test@test2.com", TenantId = TestTenant2Id, NormalizedUserName = "TEST@TEST2.COM" }
                );

            dbContext.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = Tenant1Admin, RoleId = TenantAdminRoleId },
                new IdentityUserRole<string> { UserId = Tenant2Admin, RoleId = TenantAdminRoleId },
                new IdentityUserRole<string> { UserId = Tenant1User, RoleId = TenantUserRoleId },
                new IdentityUserRole<string> { UserId = "dbb3eaec-20ac-49e1-8f82-c35ee195be25", RoleId = TenantUserRoleId },
                new IdentityUserRole<string> { UserId = "12899f34-7d73-42be-8cc6-2d503dbe40f6", RoleId = TenantUserRoleId }
                );

            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            var tenants = new[] { TestTenant1Id, TestTenant2Id };

            await dbContext.Users.Where(u => tenants.Contains(u.TenantId)).ExecuteDeleteAsync();
            await dbContext.Tenants.Where(t => tenants.Contains(t.Id)).ExecuteDeleteAsync();

            scope.Dispose();
            await factory.DisposeAsync();
        }
    }
}

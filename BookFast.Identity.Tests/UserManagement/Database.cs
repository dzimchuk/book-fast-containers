using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure;
using BookFast.Security;
using BookFast.TestInfrastructure.IntegrationTest;
using Microsoft.AspNetCore.Identity;
using Quartz;

namespace BookFast.Identity.Tests.UserManagement
{
    internal static class Database
    {
        public const string UserFromAnotherTenant = "fbed2d3d-4b81-4fe9-8e00-c35d91c69dfa";
        public const string CallerTenantAdmin = "6fcfce13-1695-4805-b1c5-d8b172cc30cd";
        public const string CallerTenantUser = "4928199a-7ec5-43dd-b9cc-14342a145284";

        public static HttpClient SeedAndGetHttpClient(this ApiFixture<Program, IdentityContext> fixture)
        {
            fixture.Seed(context =>
            {
                context.Seed();
            });

            return fixture.GetHttpClient(Roles.TenantAdmin);
        }

        public static HttpClient GetHttpClient(this ApiFixture<Program, IdentityContext> fixture, string callerRole)
        {
            fixture.UseConfiguration(new FixtureConfiguration(Role: callerRole, UserId: CallerTenantAdmin));

            return fixture.GetHttpClient(services =>
            {
                // this prevents DbContext from being disposed upon each request
                services.AddSingleton<IDbContext>(fixture.DbContext);

                // override authentication scheme required by authorization policies
                services.AddAuthorization(options => AuthorizationPolicies.Register(options, Constants.AuthenticationScheme));

                // Reconfigure the Quartz.NET service to not block shutdown.
                services.AddQuartzHostedService(options => options.WaitForJobsToComplete = false);
            });
        }

        public static void Seed(this IdentityContext context)
        {
            const string defaultTenantId = "00000000-0000-0000-0000-000000000000";
            const string callerTenantId = Constants.CallerTenant;
            const string anotherTenantId = "7e77c709-9494-4722-972f-5f7bc57ab37c";

            const string superAdminRoleId = "94619769-8a95-4db2-9057-6847ad54faa5";
            const string tenantAdminRoleId = "059150b1-e944-4987-bce4-f2f7df5a52dc";
            const string tenantUserRoleId = "3ecbef00-09ed-4e09-8eb6-e4a61c7d2da0";

            context.Tenants.AddRange(
                new Tenant { Id = defaultTenantId, Name = "Default" },
                new Tenant { Id = callerTenantId, Name = "Caller tenant" },
                new Tenant { Id = anotherTenantId, Name = "Another tenant" }
                );

            context.Roles.AddRange(
                new Role { Id = superAdminRoleId, Name = "super-admin", NormalizedName = "SUPER-ADMIN" },
                new Role { Id = tenantAdminRoleId, Name = "tenant-admin", NormalizedName = "TENANT-ADMIN" },
                new Role { Id = tenantUserRoleId, Name = "tenant-user", NormalizedName = "TENANT-USER" }
                );

            context.Users.AddRange(
                new User { Id = "dc4695df-8eea-4b60-966c-3c639f5c2659", UserName = "super-admin@super-admin.com", TenantId = defaultTenantId, NormalizedUserName = "SUPER-ADMIN@SUPER-ADMIN.COM" },
                new User { Id = CallerTenantAdmin, UserName = "test1@test.com", TenantId = callerTenantId, NormalizedUserName = "TEST1@TEST.COM" },
                new User { Id = "9a8c2b03-1457-440d-97b2-d985ea543740", UserName = "test3@gmail.com", TenantId = callerTenantId, NormalizedUserName = "TEST3@GMAIL.COM" },
                new User { Id = CallerTenantUser, UserName = "test2@test.com", TenantId = callerTenantId, NormalizedUserName = "TEST2@TEST.COM" },
                new User { Id = UserFromAnotherTenant, UserName = "test4@test.com", TenantId = anotherTenantId, NormalizedUserName = "TEST4@TEST.COM" }
                );

            context.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = "dc4695df-8eea-4b60-966c-3c639f5c2659", RoleId = superAdminRoleId },
                new IdentityUserRole<string> { UserId = CallerTenantAdmin, RoleId = tenantAdminRoleId },
                new IdentityUserRole<string> { UserId = "9a8c2b03-1457-440d-97b2-d985ea543740", RoleId = tenantUserRoleId },
                new IdentityUserRole<string> { UserId = CallerTenantUser, RoleId = tenantUserRoleId },
                new IdentityUserRole<string> { UserId = UserFromAnotherTenant, RoleId = tenantAdminRoleId }
                );
        }
    }
}
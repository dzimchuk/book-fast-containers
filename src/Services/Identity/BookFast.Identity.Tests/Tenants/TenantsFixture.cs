using BookFast.Common.Application.Integration;
using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BookFast.Identity.Tests.Tenants
{
    public class TenantsFixture : IAsyncLifetime
    {
        public const string TestTenantId = Constants.CallerTenant;
        public const string TestTenantName = "Test tenant";

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly IdentityContext dbContext;

        public HttpClient HttpClient => httpClient;

        public IDbContext DbContext => dbContext;

        public TenantsFixture(ApiFixture<Program> fixture)
        {
            httpClient = fixture.CreateHttpClient(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.GlobalAdmin
                });

                services.AddSingleton(Mock.Of<IMailNotificationQueue>());
            });

            scope = fixture.ServiceProvider.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        }

        public async Task InitializeAsync()
        {
            dbContext.Tenants.AddRange(
                [
                    new Tenant
                    {
                        Id = TestTenantId,
                        Name = TestTenantName
                    }
                ]);

            dbContext.Users.Add(new User { Id = "2710da43-aedd-4fd1-b7f2-7094c2018dbc", Email = "user@test.com", UserName = "user@test.com", TenantId = TestTenantId, NormalizedUserName = "USER@TEST.COM" });

            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Users.Where(u => u.TenantId == TestTenantId).ExecuteDeleteAsync();
            await dbContext.Tenants.Where(t => t.Id == TestTenantId).ExecuteDeleteAsync();
           
            scope.Dispose();
        }
    }
}

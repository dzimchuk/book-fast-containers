using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.Identity.Core;
using BookFast.Identity.Core.Models;
using BookFast.Identity.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

            await dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Tenants.Where(t => t.Id == TestTenantId).ExecuteDeleteAsync();
           
            scope.Dispose();
        }
    }
}

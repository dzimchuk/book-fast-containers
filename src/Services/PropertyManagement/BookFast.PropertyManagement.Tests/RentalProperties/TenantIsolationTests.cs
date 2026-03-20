using BookFast.Common.Application.Security;
using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    [Collection(nameof(IntegrationTestCollection))]
    public class TenantIsolationTests : IAsyncLifetime
    {
        private const string OtherTenantId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
        private const string baseUrl = "/api/properties";

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        private Guid propertyId;

        public TenantIsolationTests(ApiFixture<Program> fixture)
        {
            httpClient = fixture.CreateHttpClient(services =>
            {
                services.AddSingleton(new TestSecurityContext
                {
                    UserId = Constants.UserId,
                    Role = Roles.TenantUser,
                    TenantId = OtherTenantId
                });
            });

            scope = fixture.ServiceProvider.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<PropertyManagementContext>();
        }

        public async Task InitializeAsync()
        {
            var property = Property.NewProperty(
                Constants.CallerTenant,
                "Tenant1 Property",
                "Belongs to CallerTenant",
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null);

            property.Id = Guid.CreateVersion7();

            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            propertyId = property.Id;
        }

        [Fact]
        public async Task GetProperty_ReturnsNotFound()
        {
            var response = await httpClient.GetAsync($"{baseUrl}/{propertyId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task UpdateProperty_ReturnsNotFound()
        {
            var command = new UpdatePropertyCommand
            {
                Name = "Hijacked",
                Address = new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701")
            };

            var response = await httpClient.PutAsJsonAsync($"{baseUrl}/{propertyId}", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task DeleteProperty_ReturnsNotFound()
        {
            var response = await httpClient.DeleteAsync($"{baseUrl}/{propertyId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task ListProperties_ReturnsEmpty()
        {
            var response = await httpClient.GetAsync(baseUrl);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await response.ShouldBeEquivalentToFile();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Properties
                .Where(p => p.Id == propertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}

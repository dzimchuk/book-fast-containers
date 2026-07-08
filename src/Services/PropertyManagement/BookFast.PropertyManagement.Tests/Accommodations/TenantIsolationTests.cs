using BookFast.Common.Application.Security;
using BookFast.Common.Domain;
using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application.Accommodations.CreateAccommodation;
using BookFast.PropertyManagement.Application.Accommodations.UpdateAccommodation;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.Accommodations
{
    [Collection(nameof(IntegrationTestCollection))]
    public class TenantIsolationTests : IAsyncLifetime
    {
        private const string OtherTenantId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        private readonly HttpClient httpClient;
        private readonly IServiceScope scope;
        private readonly PropertyManagementContext dbContext;

        private Guid propertyId;
        private Guid accommodationId;

        public TenantIsolationTests(PropertyManagementApiFixture fixture)
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
                null,
                new Address("USA", "Texas", "Austin", "100 Congress Ave", "78701"),
                new Location(30.2672, -97.7431),
                null,
                null);

            property.Id = Guid.CreateVersion7();
            dbContext.Properties.Add(property);
            await dbContext.SaveChangesAsync();

            propertyId = property.Id;

            var accommodation = Accommodation.NewAccommodation(
                Constants.CallerTenant,
                propertyId,
                "Tenant1 Room",
                null,
                1,
                null,
                5,
                new PriceRange(new Money(100m, "USD"), new Money(100m, "USD")),
                null);

            accommodation.Id = Guid.CreateVersion7();
            dbContext.Accommodations.Add(accommodation);
            await dbContext.SaveChangesAsync();

            accommodationId = accommodation.Id;
        }

        [Fact]
        public async Task CreateAccommodation_ReturnsNotFound()
        {
            var command = new CreateAccommodationCommand
            {
                Name = "Hijacked Room",
                Bedrooms = 1,
                Quantity = 1,
                PriceRange = new PriceRange(new Money(50m, "USD"), new Money(50m, "USD"))
            };

            var response = await httpClient.PostAsJsonAsync($"/api/properties/{propertyId}/accommodations", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task GetAccommodation_ReturnsNotFound()
        {
            var response = await httpClient.GetAsync($"/api/properties/{propertyId}/accommodations/{accommodationId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task UpdateAccommodation_ReturnsNotFound()
        {
            var command = new UpdateAccommodationCommand
            {
                Name = "Hijacked",
                Bedrooms = 1,
                Quantity = 1,
                PriceRange = new PriceRange(new Money(50m, "USD"), new Money(50m, "USD"))
            };

            var response = await httpClient.PutAsJsonAsync($"/api/properties/{propertyId}/accommodations/{accommodationId}", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task DeleteAccommodation_ReturnsNotFound()
        {
            var response = await httpClient.DeleteAsync($"/api/properties/{propertyId}/accommodations/{accommodationId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task ListAccommodations_ReturnsEmpty()
        {
            var response = await httpClient.GetAsync($"/api/properties/{propertyId}/accommodations");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await response.ShouldBeEquivalentToFile();
        }

        public async Task DisposeAsync()
        {
            await dbContext.Accommodations
                .Where(a => a.PropertyId == propertyId)
                .ExecuteDeleteAsync();

            await dbContext.Properties
                .Where(p => p.Id == propertyId)
                .ExecuteDeleteAsync();

            scope.Dispose();
        }
    }
}

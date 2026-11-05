using BookFast.Common.TestInfrastructure;
using BookFast.Common.TestInfrastructure.IntegrationTest;
using BookFast.PropertyManagement.Application.RentalProperties.CreateProperty;
using BookFast.PropertyManagement.Domain;
using BookFast.PropertyManagement.Integration;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    [Collection(nameof(IntegrationTestCollection))]
    public class CreatePropertyTests(RentalPropertiesFixture fixture) : IClassFixture<RentalPropertiesFixture>, IAsyncLifetime
    {
        private const string baseUrl = "/api/properties";

        private Guid? newPropertyId = null;

        private static readonly Address ValidAddress = new("USA", "Texas", "Austin", "100 Congress Ave", "78701");

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new CreatePropertyCommand()],
            ["NameTooShort", new CreatePropertyCommand { Name = "ab", Address = ValidAddress }],
            ["FacilityNotPropertyScoped", new CreatePropertyCommand
            {
                Name = "New Property", Address = ValidAddress,
                Facilities = [Facility.Refrigerator]
            }]
        ];

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, CreatePropertyCommand command)
        {
            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName, partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var command = new CreatePropertyCommand
            {
                Name = "New Property",
                Description = "A brand new property",
                Address = ValidAddress
            };

            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/properties/(?<id>[0-9a-fA-F-]+)");
            Assert.True(match.Success);

            newPropertyId = Guid.Parse(match.Groups["id"].Value);

            var property = await fixture.DbContext.Properties.FindAsync(newPropertyId.Value);
            Assert.NotNull(property);
            Assert.Equal("New Property", property.Name);
            Assert.Equal("A brand new property", property.Description);
            Assert.Equal(Constants.CallerTenant, property.TenantId);
            Assert.True(property.IsActive);

            var publishedEvent = await fixture.IntegrationEvents.WaitForEventAsync<PropertyCreatedEvent>(
                e => e.PropertyId == newPropertyId.Value);

            Assert.NotEqual(Guid.Empty, publishedEvent.EventId);
            Assert.NotEqual(default, publishedEvent.OccurredAt);

            JsonSerializer.SerializeToNode(publishedEvent).ShouldBeEquivalentToFile(caseName: "PropertyCreatedEvent", partial: true);

            Assert.False(fixture.IntegrationEvents.HasCaptured<AccommodationCreatedEvent>(e => e.PropertyId == newPropertyId.Value));
        }

        [Fact]
        public async Task Facilities_Success()
        {
            var command = new CreatePropertyCommand
            {
                Name = "Property With Facilities",
                Address = ValidAddress,
                Facilities = [Facility.Parking, Facility.WiFi]
            };

            var response = await fixture.HttpClient.PostAsJsonAsync(baseUrl, command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var match = Regex.Match(response.Headers.Location.OriginalString, @".*/properties/(?<id>[0-9a-fA-F-]+)");
            newPropertyId = Guid.Parse(match.Groups["id"].Value);

            var property = await fixture.DbContext.Properties.FindAsync(newPropertyId.Value);
            Assert.NotNull(property);
            Assert.Equal([Facility.Parking, Facility.WiFi], property.Facilities);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            if (newPropertyId.HasValue)
            {
                await fixture.DbContext.Properties
                    .Where(p => p.Id == newPropertyId.Value)
                    .ExecuteDeleteAsync();
            }
        }
    }
}

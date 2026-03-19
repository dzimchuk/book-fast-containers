using BookFast.Common.TestInfrastructure;
using BookFast.PropertyManagement.Application.RentalProperties.UpdateProperty;
using BookFast.PropertyManagement.Domain;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace BookFast.PropertyManagement.Tests.RentalProperties
{
    [Collection(nameof(IntegrationTestCollection))]
    public class UpdatePropertyTests(RentalPropertiesFixture fixture) : IClassFixture<RentalPropertiesFixture>
    {
        private const string baseUrl = "/api/properties";

        private static readonly Address ValidAddress = new("USA", "Texas", "Austin", "100 Congress Ave", "78701");

        public static IEnumerable<object[]> ValidationData =>
        [
            ["EmptyParameters", new UpdatePropertyCommand()],
            ["NameTooShort", new UpdatePropertyCommand { Name = "ab", Address = ValidAddress }]
        ];

        [Theory, MemberData(nameof(ValidationData))]
        public async Task Validation(string caseName, UpdatePropertyCommand command)
        {
            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{fixture.Property1Id}", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            await response.ShouldBeEquivalentToFile(caseName: caseName, partial: true);
        }

        [Fact]
        public async Task PropertyNotFound()
        {
            var command = new UpdatePropertyCommand { Name = "Updated", Address = ValidAddress };

            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/9999", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            await response.ShouldBeEquivalentToFile(partial: true);
        }

        [Fact]
        public async Task Success()
        {
            var command = new UpdatePropertyCommand
            {
                Name = "Updated Mountain Retreat",
                Description = "Updated description",
                Address = ValidAddress
            };

            var response = await fixture.HttpClient.PutAsJsonAsync($"{baseUrl}/{fixture.Property1Id}", command);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var property = await fixture.DbContext.Properties.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == fixture.Property1Id);

            Assert.NotNull(property);
            Assert.Equal("Updated Mountain Retreat", property.Name);
            Assert.Equal("Updated description", property.Description);
        }
    }
}

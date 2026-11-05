namespace BookFast.Booking.Tests;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<BookingApiFixture>
{
}

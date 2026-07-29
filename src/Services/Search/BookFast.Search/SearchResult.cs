using BookFast.Common.Domain;

namespace BookFast.Search
{
    internal class SearchResult
    {
        public Guid AccommodationId { get; init; }
        public Guid PropertyId { get; init; }

        public string Name { get; init; }
        public string Description { get; init; }
        public int? Bedrooms { get; init; }
        public string[] Images { get; init; }
        public string[] Facilities { get; init; }

        public string PropertyName { get; set; }
        public string PropertyDescription { get; set; }

        public string City { get; init; }
        public string Country { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }

        public Money MinPrice { get; init; }
        public Money MaxPrice { get; init; }
    }
}

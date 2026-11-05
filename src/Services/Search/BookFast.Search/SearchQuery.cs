namespace BookFast.Search
{
    internal class SearchQuery
    {
        public string Query { get; init; }

        public int? Bedrooms { get; init; }

        public string[] Facilities { get; init; } = [];

        public string City { get; init; }

        public string Country { get; init; }

        public SearchSort Sort { get; init; } = SearchSort.Relevance;

        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

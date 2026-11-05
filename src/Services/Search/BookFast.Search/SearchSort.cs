using System.Text.Json.Serialization;

namespace BookFast.Search
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    internal enum SearchSort
    {
        Relevance
    }
}

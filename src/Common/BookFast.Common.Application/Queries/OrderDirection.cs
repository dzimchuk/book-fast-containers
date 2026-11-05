using System.Text.Json.Serialization;

namespace BookFast.Common.Application.Queries
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderDirection
    {
        Asc,
        Desc
    }
}

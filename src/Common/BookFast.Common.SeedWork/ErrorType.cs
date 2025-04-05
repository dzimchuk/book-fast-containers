using System.Text.Json.Serialization;

namespace BookFast.Common.SeedWork;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4
}

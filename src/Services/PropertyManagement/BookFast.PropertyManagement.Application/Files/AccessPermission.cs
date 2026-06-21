using System.Text.Json.Serialization;

namespace BookFast.PropertyManagement.Application.Files
{
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AccessPermission
    {
        Read = 1,
        Write = 2,
        Delete = 4
    }
}

using System.Text.Json;

namespace BookFast.PropertyManagement.Infrastructure
{
    internal static class Extensions
    {
        public static string ToJson(this string[] array)
        {
            return array != null ? JsonSerializer.Serialize(array) : null;
        }

        public static string[] ToStringArray(this string json)
        {
            return !string.IsNullOrWhiteSpace(json) ? JsonSerializer.Deserialize<string[]>(json) : null;
        }
    }
}

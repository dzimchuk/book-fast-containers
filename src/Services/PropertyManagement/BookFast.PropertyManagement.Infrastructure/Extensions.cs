using System.Text.Json;
using BookFast.PropertyManagement.Domain;

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

        public static string ToJson(this Facility[] facilities)
        {
            return facilities != null ? JsonSerializer.Serialize(facilities) : null;
        }

        public static Facility[] ToFacilityArray(this string json)
        {
            return !string.IsNullOrWhiteSpace(json) ? JsonSerializer.Deserialize<Facility[]>(json) : [];
        }
    }
}

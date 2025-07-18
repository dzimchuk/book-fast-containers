using System.Globalization;
using System.Reflection;

namespace BookFast.Common.TestInfrastructure
{
    public static class RequestHelpers
    {
        public static IDictionary<string, string> ToKeyValue(this object payload)
        {
            if (payload == null)
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            var properties = payload.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(payload);
                    result[property.Name] = FormatValue(property.PropertyType, value);
                }
            }

            return result;
        }

        private static string FormatValue(Type propertyType, object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                return ((DateTime)value).ToString("o", CultureInfo.InvariantCulture);
            }

            if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
            {
                return ((DateTimeOffset)value).ToString("o", CultureInfo.InvariantCulture);
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static string ToUrlEncodedString(this object payload)
        {
            var keyValueContent = payload.ToKeyValue();
            if (keyValueContent == null)
            {
                return null;
            }

            var formUrlEncodedContent = new FormUrlEncodedContent(keyValueContent);
            return formUrlEncodedContent.ReadAsStringAsync().GetAwaiter().GetResult();
        }
    }
}

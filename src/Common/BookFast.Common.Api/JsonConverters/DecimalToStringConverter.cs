using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace BookFast.Common.Api.JsonConverters
{
    public class DecimalToStringConverter : JsonConverter<decimal>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(decimal);
        }

        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (decimal.TryParse(reader.GetString(), out decimal value))
                {
                    return value;
                }
                throw new JsonException("Unable to parse decimal value from string.");
            }
            return reader.GetDecimal();
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("0.########", CultureInfo.InvariantCulture));
        }
    }
}

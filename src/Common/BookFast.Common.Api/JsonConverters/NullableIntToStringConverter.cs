using System.Text.Json.Serialization;
using System.Text.Json;

namespace BookFast.Common.Api.JsonConverters
{
    public class NullableIntToStringConverter : JsonConverter<int?>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(int?);
        }

        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                if (int.TryParse(stringValue, out int value))
                {
                    return value;
                }
                throw new JsonException("Unable to parse int value from string.");
            }

            return reader.GetInt32();
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteNumberValue(value.Value);
            }
        }
    }
}

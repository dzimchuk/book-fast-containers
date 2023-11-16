using Newtonsoft.Json.Linq;
using System.Globalization;

namespace BookFast.TestInfrastructure
{
    public static class RequestHelpers
    {
        public static IDictionary<string, string> ToKeyValue(this object payload)
        {
            if (payload == null)
            {
                return null;
            }

            var token = payload as JToken;
            if (token == null)
            {
                return JObject.FromObject(payload).ToKeyValue();
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = child.ToKeyValue();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                            jValue?.ToString("o", CultureInfo.InvariantCulture) :
                            jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
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

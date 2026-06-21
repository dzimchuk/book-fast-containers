using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System.Text.Json.Nodes;

namespace BookFast.Common.TestInfrastructure
{
    public static class JsonExtensions
    {
        public static async Task ShouldBeEquivalentToFile(this HttpResponseMessage response,
            string caseName = null,
            bool partial = false,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            var responseText = await response.Content.ReadAsStringAsync();
            var responsePayload = JsonNode.Parse(responseText);

            ShouldBeEquivalentToFileInternal(responsePayload, memberName, sourceFilePath, caseName, partial);
        }

        public static void ShouldBeEquivalentToFile(this JsonNode actual,
            string caseName = null,
            bool partial = false,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            ShouldBeEquivalentToFileInternal(actual, memberName, sourceFilePath, caseName, partial);
        }

        private static void ShouldBeEquivalentToFileInternal(JsonNode actual, string memberName, string sourceFilePath, string caseName = null, bool partial = false)
        {
            using (new AssertionScope(new CustomAssertionStrategy(() => WriteJson(actual, memberName, sourceFilePath, caseName))))
            {
                var expected = ReadJson(memberName, sourceFilePath, caseName);
                if (partial)
                    actual.Should().PartiallyMatchesWith(expected);
                else
                    actual.Should().DeepEqualsWith(expected);
            }
        }

        public static JsonNodeAssertions Should(this JsonNode instance)
        {
            return new JsonNodeAssertions(instance, AssertionChain.GetOrCreate());
        }

        private static void WriteJson(JsonNode actual, string memberName, string sourceFilePath, string caseName = null)
        {
            var path = Path.GetDirectoryName(sourceFilePath);
            var fileName = !string.IsNullOrWhiteSpace(caseName)
                ? Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_{caseName}_actual.json")
                : Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_actual.json");

            File.WriteAllText(fileName, actual.ToString());
        }

        private static JsonNode ReadJson(string memberName, string sourceFilePath, string caseName = null)
        {
            var path = Path.GetDirectoryName(sourceFilePath);
            var fileName = !string.IsNullOrWhiteSpace(caseName)
                ? Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_{caseName}.json")
                : Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}.json");

            if (Path.Exists(fileName))
            {
                var expected = File.ReadAllText(fileName);
                return JsonNode.Parse(expected);
            }
            else
            {
                return JsonNode.Parse("{}");
            }
        }
    }

    public class JsonNodeAssertions : ReferenceTypeAssertions<JsonNode, JsonNodeAssertions>
    {
        public JsonNodeAssertions(JsonNode instance, AssertionChain chain)
            : base(instance, chain)
        {
        }

        protected override string Identifier => "json";

        [CustomAssertion]
        public AndConstraint<JsonNodeAssertions> DeepEqualsWith(
            JsonNode expected, string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .ForCondition(JsonNode.DeepEquals(Subject, expected))
                .FailWith("Actual JsonNode does not match expected JsonNode. \nActual:\n{0}\nExpected:\n{1}", Subject.ToString(), expected.ToString());

            return new AndConstraint<JsonNodeAssertions>(this);
        }

        [CustomAssertion]
        public AndConstraint<JsonNodeAssertions> PartiallyMatchesWith(
            JsonNode expected, string because = "", params object[] becauseArgs)
        {
            CurrentAssertionChain
                .BecauseOf(because, becauseArgs)
                .ForCondition(IsSubsetOf(Subject, expected))
                .FailWith("Actual JsonNode does not contain all expected properties. \nActual:\n{0}\nExpected:\n{1}", Subject.ToString(), expected.ToString());

            return new AndConstraint<JsonNodeAssertions>(this);
        }

        private static bool IsSubsetOf(JsonNode actual, JsonNode expected)
        {
            if (expected is JsonObject expectedObj)
            {
                if (actual is not JsonObject actualObj)
                    return false;

                foreach (var kvp in expectedObj)
                {
                    if (!actualObj.TryGetPropertyValue(kvp.Key, out var actualValue))
                        return false;

                    if (!IsSubsetOf(actualValue, kvp.Value))
                        return false;
                }
                return true;
            }

            return JsonNode.DeepEquals(actual, expected);
        }
    }
}

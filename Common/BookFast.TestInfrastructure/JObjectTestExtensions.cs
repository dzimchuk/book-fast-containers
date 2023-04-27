using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BookFast.TestInfrastructure
{
    public static class JObjectTestExtensions
    {
        public static bool DeepEqualsWithFile(this JObject jObject,
                string valueKey = null,
                [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePathStr = null)
        {
            var path = Path.GetDirectoryName(sourceFilePathStr);
            var fileNameWithoutExt = Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePathStr)}_{memberName}{(valueKey != null ? "__" + valueKey : "")}");

            string jsonFileName = $"{fileNameWithoutExt}.json";

            return jObject.DeepEqualsWithFile(jsonFileName);
        }

        public static bool DeepEqualsWithFile(this JObject jObject, string jsonFileName)
        {
            var equals = false;

            if (File.Exists(jsonFileName))
            {
                try
                {
                    var expectedStr = File.ReadAllText(jsonFileName);

                    var expectedResponse = JObject.Parse(expectedStr);

                    equals = JToken.DeepEquals(expectedResponse, jObject);
                }
                catch (JsonReaderException)
                {
                }
            }

            if (!equals)
            {
                var projectLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                projectLocation = projectLocation.Substring(0, projectLocation.IndexOf("bin"));

                var actualJsonFileName = Path.Combine(projectLocation, Path.GetDirectoryName(jsonFileName), $"{Path.GetFileNameWithoutExtension(jsonFileName)}_actual.json");
                File.WriteAllText(actualJsonFileName, jObject.ToString());
            }

            return equals;
        }
    }
}

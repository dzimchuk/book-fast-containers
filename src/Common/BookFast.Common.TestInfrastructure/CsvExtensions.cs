using CsvHelper;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Globalization;

namespace BookFast.Common.TestInfrastructure
{
    public static class CsvExtensions
    {
        public static void ShouldBeEquivalentToCsv<T>(this IEnumerable<T> actualRecords,
            string caseName = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            using (new AssertionScope(new CustomAssertionStrategy(() => WriteCsv(actualRecords, memberName, sourceFilePath, caseName))))
            {
                var expectedRecords = ReadCsv<T>(memberName, sourceFilePath, caseName);
                actualRecords.Should().BeEquivalentTo(expectedRecords);
            }
        }

        private static void WriteCsv<T>(IEnumerable<T> records, string memberName, string sourceFilePath, string caseName = null)
        {
            var path = Path.GetDirectoryName(sourceFilePath);
            var fileName = !string.IsNullOrWhiteSpace(caseName)
                ? Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_{caseName}_actual.csv")
                : Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_actual.csv");

            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }
        }

        private static IEnumerable<T> ReadCsv<T>(string memberName, string sourceFilePath, string caseName = null)
        {
            var path = Path.GetDirectoryName(sourceFilePath);
            var fileName = !string.IsNullOrWhiteSpace(caseName)
                ? Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}_{caseName}.csv")
                : Path.Combine(path, $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{memberName}.csv");

            if (Path.Exists(fileName))
            {
                using (var reader = new StreamReader(fileName))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.Add(string.Empty);
                    return csv.GetRecords<T>().ToArray();
                }
            }
            else
            {
                return [];
            }
        }
    }
}

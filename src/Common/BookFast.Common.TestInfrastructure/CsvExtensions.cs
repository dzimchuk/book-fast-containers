using CsvHelper;
using FluentAssertions;
using FluentAssertions.Execution;

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

            using (var csv = CsvWriter.ToFile(fileName))
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
                var options = new CsvReaderOptions
                {
                    Separator = ',',
                    NullValues = [string.Empty]
                };

                using (var csv = CsvReader.FromFile(fileName, options))
                {
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

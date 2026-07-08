using System.Globalization;

namespace BookFast.Common.Domain
{
    public static class CurrencyCodes
    {
        private static readonly Lazy<HashSet<string>> knownCodes = new(() =>
            CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture =>
                {
                    try
                    {
                        return new RegionInfo(culture.Name).ISOCurrencySymbol;
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                })
                .Where(code => code != null)
                .ToHashSet(StringComparer.OrdinalIgnoreCase));

        public static bool IsValid(string currency) =>
            !string.IsNullOrWhiteSpace(currency) && knownCodes.Value.Contains(currency);
    }
}

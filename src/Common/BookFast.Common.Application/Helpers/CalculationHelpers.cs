namespace BookFast.Common.Application.Helpers
{
    public static class CalculationHelpers
    {
        public static decimal RoundToWhole(this decimal value) => value.Round(0);

        public static decimal Round(this decimal value, int decimals) =>
            Math.Round(value, decimals, MidpointRounding.AwayFromZero);

        public static decimal AsPercentage(this decimal ratio) => ratio * 100M;
    }
}

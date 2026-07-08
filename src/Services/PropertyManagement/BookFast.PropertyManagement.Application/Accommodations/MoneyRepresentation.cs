using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Application.Accommodations
{
    public class MoneyRepresentation
    {
        /// <summary>
        /// Amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ISO 4217 currency code
        /// </summary>
        public string Currency { get; set; }

        public static MoneyRepresentation Map(Money money) =>
            money is null
                ? null
                : new MoneyRepresentation
                {
                    Amount = money.Amount,
                    Currency = money.Currency
                };
    }
}

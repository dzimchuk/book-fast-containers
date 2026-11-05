using BookFast.Common.Domain;

namespace BookFast.Booking.Application.Reservations
{
    public class MoneyRepresentation
    {
        public decimal Amount { get; set; }

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

using BookFast.Common.Domain;
using FluentValidation;

namespace BookFast.PropertyManagement.Application.Accommodations
{
    public class MoneyValidator : AbstractValidator<Money>
    {
        public MoneyValidator()
        {
            RuleFor(money => money.Amount).GreaterThanOrEqualTo(decimal.Zero);
            RuleFor(money => money.Currency).NotEmpty().Must(CurrencyCodes.IsValid)
                .WithErrorCode("InvalidCurrency")
                .WithMessage("'{PropertyName}' is not a valid ISO 4217 currency code.");
        }
    }
}

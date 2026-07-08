using BookFast.Common.Domain;
using FluentValidation;

namespace BookFast.PropertyManagement.Application.Accommodations
{
    public class PriceRangeValidator : AbstractValidator<PriceRange>
    {
        public PriceRangeValidator()
        {
            When(range => range.MinPrice is not null, () =>
                RuleFor(range => range.MinPrice).SetValidator(new MoneyValidator()));

            When(range => range.MaxPrice is not null, () =>
                RuleFor(range => range.MaxPrice).SetValidator(new MoneyValidator()));

            RuleFor(range => range)
                .Must(HaveMatchingCurrencies)
                .WithErrorCode("PriceRangeCurrencyMismatch")
                .WithMessage("'Min Price' and 'Max Price' must use the same currency.")
                .Must(HaveCoherentBounds)
                .WithErrorCode("PriceRangeIncoherentBounds")
                .WithMessage("'Min Price' must not be greater than 'Max Price'.");
        }

        private static bool HaveMatchingCurrencies(PriceRange range) =>
            range.MinPrice is null || range.MaxPrice is null ||
            string.Equals(range.MinPrice.Currency, range.MaxPrice.Currency, StringComparison.OrdinalIgnoreCase);

        private static bool HaveCoherentBounds(PriceRange range) =>
            range.MinPrice is null || range.MaxPrice is null ||
            range.MinPrice.Amount <= range.MaxPrice.Amount;
    }
}

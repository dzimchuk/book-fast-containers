using BookFast.Common.Domain;

namespace BookFast.PropertyManagement.Application.Accommodations
{
    public class PriceRangeRepresentation
    {
        /// <summary>
        /// Minimum price
        /// </summary>
        public MoneyRepresentation MinPrice { get; set; }

        /// <summary>
        /// Maximum price
        /// </summary>
        public MoneyRepresentation MaxPrice { get; set; }

        public static PriceRangeRepresentation Map(PriceRange priceRange) =>
            priceRange is null || priceRange.IsEmpty
                ? null
                : new PriceRangeRepresentation
                {
                    MinPrice = MoneyRepresentation.Map(priceRange.MinPrice),
                    MaxPrice = MoneyRepresentation.Map(priceRange.MaxPrice)
                };
    }
}

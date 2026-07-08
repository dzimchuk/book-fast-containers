namespace BookFast.Common.Domain
{
    public record PriceRange
    {
        public Money MinPrice { get; init; }
        public Money MaxPrice { get; init; }

        public bool IsEmpty => MinPrice is null && MaxPrice is null;

        public PriceRange()
        {
        }

        public PriceRange(Money minPrice, Money maxPrice)
        {
            MinPrice = minPrice;
            MaxPrice = maxPrice;
        }
    }
}

namespace BookFast.Booking.Domain
{
    public record Stay(DateOnly CheckIn, DateOnly CheckOut)
    {
        public int Nights => CheckOut.DayNumber - CheckIn.DayNumber;

        public bool Overlaps(Stay other) => CheckIn < other.CheckOut && other.CheckIn < CheckOut;

        public bool Includes(DateOnly night) => night >= CheckIn && night < CheckOut;
    }
}

namespace BookFast.TestInfrastructure.AsyncQueryableMock
{
    public static class EnumerableExtensions
    {
        public static IQueryable<T> MockAsyncQueryable<T>(this IEnumerable<T> data)
        {
            return new TestAsyncEnumerable<T>(data);
        }
    }
}

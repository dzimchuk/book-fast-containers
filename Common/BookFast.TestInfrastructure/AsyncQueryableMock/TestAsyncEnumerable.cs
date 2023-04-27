using System.Linq.Expressions;

namespace BookFast.TestInfrastructure.AsyncQueryableMock
{
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }
    }
}

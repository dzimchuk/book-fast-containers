namespace BookFast.TestInfrastructure.AsyncQueryableMock
{
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> enumerator;

        public TestAsyncEnumerator(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator;
        }

        public T Current => enumerator.Current;

        public ValueTask DisposeAsync()
        {
            enumerator.Dispose();
            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(enumerator.MoveNext());
        }
    }
}

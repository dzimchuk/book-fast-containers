using FluentAssertions;
using FluentAssertions.Execution;
using System.Globalization;
using System.Text;

namespace BookFast.Common.TestInfrastructure
{
    internal class CustomAssertionStrategy(Action onThrow = null) : IAssertionStrategy
    {
        private readonly List<string> failureMessages = new();

        public IEnumerable<string> FailureMessages => failureMessages;

        public IEnumerable<string> DiscardFailures()
        {
            var discardedFailures = failureMessages.ToArray();
            failureMessages.Clear();
            return discardedFailures;
        }

        public void HandleFailure(string message)
        {
            failureMessages.Add(message);
        }

        public void ThrowIfAny(IDictionary<string, object> context)
        {
            if (failureMessages.Count > 0)
            {
                var builder = new StringBuilder();
                builder.AppendJoin(Environment.NewLine, failureMessages).AppendLine();

                if (context.Any())
                {
                    foreach (KeyValuePair<string, object> pair in context)
                    {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "\nWith {0}:\n{1}", pair.Key, pair.Value);
                    }
                }

                onThrow?.Invoke();

                AssertionEngine.TestFramework.Throw(builder.ToString());
            }
        }
    }
}

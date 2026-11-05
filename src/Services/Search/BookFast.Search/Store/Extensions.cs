using System.Linq.Expressions;

namespace BookFast.Search.Store
{
    internal static class Extensions
    {
        /// <summary>
        /// Combines two predicate expressions with a logical AND, substituting the second expression's
        /// parameter with the first's so the result is a single valid lambda (rather than two independent ones).
        /// </summary>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var parameter = left.Parameters[0];
            var rightBody = new ReplaceParameterVisitor(right.Parameters[0], parameter).Visit(right.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rightBody), parameter);
        }

        private class ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : base.VisitParameter(node);
        }
    }
}

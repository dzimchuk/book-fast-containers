using System.Linq.Expressions;

namespace BookFast.Common.Application.Helpers.Expressions
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            Expression rewrittenRight = ReplaceParameter(left, right);
            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left.Body, rewrittenRight), left.Parameters);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            Expression rewrittenRight = ReplaceParameter(left, right);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, rewrittenRight), left.Parameters);
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> left) =>
            Expression.Lambda<Func<T, bool>>(Expression.Not(left.Body), left.Parameters);

        private static Expression ReplaceParameter<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var visitor = new ParameterReplaceVisitor()
            {
                Target = right.Parameters[0],
                Replacement = left.Parameters[0],
            };
            return visitor.Visit(right.Body);
        }
    }
}

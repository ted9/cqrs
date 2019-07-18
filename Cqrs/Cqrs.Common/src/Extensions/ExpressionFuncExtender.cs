using System;
using System.Collections.Generic;
using System.Linq;


namespace System.Linq.Expressions
{
    internal class ParameterRebinder : ExpressionVisitor
    {
        #region Private Fields
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;
        #endregion

        #region Ctor
        internal ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }
        #endregion

        #region Internal Static Methods
        internal static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }
        #endregion

        #region Protected Methods
        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement)) {
                p = replacement;
            }
            return base.VisitParameter(p);
        }
        #endregion
    }

    public static class ExpressionFuncExtender
    {
        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);

            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);

            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }

        #region Public Methods
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.And);

        }

        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {

            return first.Compose(second, Expression.AndAlso);

        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.Or);

        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(expression.Body),
                expression.Parameters.Single()
            );
        }
        #endregion

        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression != null && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)) {
                expression = ((UnaryExpression)expression).Operand.RemoveConvert();
            }
            return expression;
        }

        public static ParameterExpression Parameter(this MemberExpression expression)
        {
            ParameterExpression parameter = expression.Expression as ParameterExpression;
            if (parameter != null) {
                return parameter;
            }
            MemberExpression member = expression.Expression as MemberExpression;
            if (member == null) {
                throw new InvalidOperationException("不支持的排序类型。");
            }
            return Parameter(member);
        }
    }
}

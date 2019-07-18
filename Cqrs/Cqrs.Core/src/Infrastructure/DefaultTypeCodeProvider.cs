using System;

namespace Cqrs.Infrastructure
{
    /// <summary>
    /// <see cref="ITypeCodeProvider"/> 的默认实现。
    /// </summary>
    public class DefaultTypeCodeProvider : AbstractTypeCodeProvider
    {
        /// <summary>
        /// 匹配的类型。
        /// </summary>
        protected override bool MatchedType(Type type)
        {
            return TypeHelper.IsAggregateRoot(type) ||
                TypeHelper.IsMessage(type) ||
                TypeHelper.IsHandlerType(type);
        }
    }
}

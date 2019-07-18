﻿
namespace Cqrs.Infrastructure
{
    /// <summary>
    /// 实体验证接口
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// 持久化之前验证
        /// </summary>
        void Validate(object context);
    }
}

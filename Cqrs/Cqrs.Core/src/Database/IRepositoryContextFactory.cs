using Cqrs.Components;
using Cqrs.Infrastructure.Repositories;

namespace Cqrs.Database
{
    /// <summary>
    /// 创建仓储上下文的工厂
    /// </summary>
    [RequiredComponent(typeof(DefaultRepositoryContextFactory))]
    public interface IRepositoryContextFactory
    {
        /// <summary>
        /// 获取当前的仓储上下文
        /// </summary>
        IRepositoryContext GetCurrentRepositoryContext();

        /// <summary>
        /// 创建一个仓储上下文
        /// </summary>
        IRepositoryContext CreateRepositoryContext();
    }
}

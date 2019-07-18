
namespace Cqrs.Database
{
    /// <summary>
    /// 仓储上下文
    /// </summary>
    public interface IRepositoryContext : Cqrs.Infrastructure.IUnitOfWork, System.IDisposable
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit(string correlationId);

        /// <summary>
        /// 获取仓储
        /// </summary>
        TRepository GetRepository<TRepository>() where TRepository : IRepository;
    }
}

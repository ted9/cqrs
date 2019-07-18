

namespace Cqrs.Database
{
    /// <summary>
    /// 表示这是一个仓储接口
    /// </summary>
    public interface IRepository
    { }


    /// <summary>
    /// 表示继承该接口的是一个仓储。
    /// </summary>
    public interface IRepository<TAggregateRoot> : IRepository
        where TAggregateRoot : class, IAggregateRoot
    {
        /// <summary>
        /// 添加聚合到仓储
        /// </summary>
        void Add(TAggregateRoot aggregateRoot);
        /// <summary>
        /// 更新聚合到仓储
        /// </summary>
        void Update(TAggregateRoot aggregateRoot);
        /// <summary>
        /// 从仓储中移除聚合
        /// </summary>
        void Remove(TAggregateRoot aggregateRoot);

        /// <summary>
        /// 根据标识id获得聚合实例
        /// </summary>
        TAggregateRoot Get<TIdentify>(TIdentify id);
        /// <summary>
        /// 查找聚合。如果不存在返回null，存在返回实例
        /// </summary>
        TAggregateRoot Find<TIdentify>(TIdentify id);
    }
}

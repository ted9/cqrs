using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace Cqrs.Infrastructure.Storage
{
    /// <summary>
    /// 数据上下文
    /// </summary>
    public interface IDataContext : IUnitOfWork, IDisposable
    {
        /// <summary>
        /// 获取跟踪的对象集合
        /// </summary>
        ICollection TrackingObjects { get; }

        /// <summary>
        /// 判断此 <paramref name="entity"/> 是否存在于当前上下文中
        /// </summary>
        bool Contains<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 从当前上下文中分离此 <paramref name="entity"/>
        /// </summary>
        void Detach<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 保存 <paramref name="entity"/> 到数据库(提交时会触发sql-insert)
        /// </summary>
        void Save<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 更新 <paramref name="entity"/> 到数据库(提交时会触发sql-update)
        /// </summary>
        void Update<TEntity>(TEntity entity) where TEntity : class;
        /// <summary>
        /// 从数据库中删除 <paramref name="entity"/>(提交时会触发sql-delete)
        /// </summary>
        void Delete<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// 获取对象
        /// </summary>
        object Get(Type type, params object[] keyValues);
        /// <summary>
        /// 获取实体信息
        /// </summary>
        TEntity Get<TEntity>(params object[] keyValues) where TEntity : class;
        /// <summary>
        /// 从数据库中刷新(触发sql-select)
        /// </summary>
        void Refresh<TEntity>(TEntity entity) where TEntity : class;



        /// <summary>
        /// 获取对数据类型已知的特定数据源的查询进行计算的功能。
        /// </summary>
        IQueryable<TEntity> CreateQuery<TEntity>() where TEntity : class;

        /// <summary>
        /// 获取当前的数据连接
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// 当数据提交后执行
        /// </summary>
        event EventHandler DataCommitted;
    }    
}

using System.Collections.Generic;

using ThinkNet.Database;
using ThinkNet.Infrastructure.Logging;
using ThinkNet.Messaging;


namespace ThinkNet.Infrastructure.Repositories
{
    /// <summary>
    /// <see cref="IRepository"/> 的默认实现。
    /// </summary>
    public class DomainRepository : RepositoryBase
    {
        private readonly IDbContextFactory _dbContextFactory;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DomainRepository(IEventBus eventBus, IMemoryCache cache, IDbContextFactory dbContextFactory, ILoggerFactory loggerFactory)
            : base(eventBus, cache, loggerFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// 从数据库中获取聚合
        /// </summary>
        protected override T GetFromStorage<T>(object aggregateId)
        {
            using (var context = _dbContextFactory.CreateDbContext()) {
                return context.Get<T>(aggregateId);
            }
        }
        /// <summary>
        /// 提交所有聚合。
        /// </summary>
        protected override void DoCommit(IEnumerable<IAggregateRoot> addedAggregateRoots, IEnumerable<IAggregateRoot> modifiedAggregateRoots, IEnumerable<IAggregateRoot> deletedAggregateRoots)
        {
            using (var context = _dbContextFactory.CreateDbContext()) {
                if (addedAggregateRoots != null)
                    addedAggregateRoots.ForEach(context.Insert);
                if (modifiedAggregateRoots != null)
                    modifiedAggregateRoots.ForEach(context.Update);
                if (deletedAggregateRoots != null)
                    deletedAggregateRoots.ForEach(context.Delete);

                context.Commit();
            }
        }
    }
}

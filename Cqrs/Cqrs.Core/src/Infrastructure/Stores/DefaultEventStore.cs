using System.Collections.Generic;
using System.Linq;

using Cqrs.EventSourcing.Streams;
using Cqrs.Infrastructure.Storage;


namespace Cqrs.Infrastructure.Stores
{
    /// <summary>
    /// <see cref="IEventStore"/> 的默认实现。
    /// </summary>
    public class DefaultEventStore : IEventStore
    {
        private readonly IDataContextFactory _dbContextFactory;
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public DefaultEventStore(IDataContextFactory dbContextFactory)
        {
            this._dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// 添加溯原事件
        /// </summary>
        public void Append(IEnumerable<EventData> events)
        {
            using (var context = _dbContextFactory.CreateDataContext()) {
                events.ForEach(context.Save);
                context.Commit();
            }
        }

        /// <summary>
        /// 判断该命令下是否存在相关事件。
        /// </summary>
        public bool IsExist(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                return false;

            using (var context = _dbContextFactory.CreateDataContext()) {
                return context.CreateQuery<EventData>()
                    .Any(p => p.CorrelationId == commandId);
            }
        }

        /// <summary>
        /// 查询该命令下的溯源事件。
        /// </summary>
        public IEnumerable<EventData> FindAll(string commandId)
        {
            using (var context = _dbContextFactory.CreateDataContext()) {
                return context.CreateQuery<EventData>()
                    .Where(p => p.CorrelationId == commandId)
                    .OrderBy(p => p.Version)
                    .ToList();
            }
        }
        /// <summary>
        /// 查询聚合的溯源事件。
        /// </summary>
        public IEnumerable<EventData> FindAll(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion)
        {
            using (var context = _dbContextFactory.CreateDataContext()) {
                return context.CreateQuery<EventData>()
                    .Where(p => p.AggregateRootId == aggregateRootId &&
                        p.AggregateRootTypeCode == aggregateRootTypeCode &&
                        p.Version > minVersion)
                    .OrderBy(p => p.Version)
                    .ToList();
            }
        }

        /// <summary>
        /// 移除该聚合的溯源事件。
        /// </summary>
        public virtual void RemoveAll(string aggregateRootId, int aggregateRootTypeCode)
        {
            using (var context = _dbContextFactory.CreateDataContext()) {
                context.CreateQuery<EventData>()
                    .Where(p => p.AggregateRootId == aggregateRootId &&
                        p.AggregateRootTypeCode == aggregateRootTypeCode)
                    .ToList().ForEach(context.Delete);
                context.Commit();
            }
        }
    }
}

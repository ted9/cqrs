using System.Collections.Generic;
using System.Linq;

using ThinkNet.Infrastructure;
using ThinkNet.Infrastructure.Logging;
using ThinkNet.Messaging;


namespace ThinkNet.Database
{
    /// <summary>
    /// <see cref="IRepository"/> 的抽象实现。
    /// </summary>
    public abstract class RepositoryBase : IRepository
    {
        private readonly IEventBus _eventBus;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected RepositoryBase(IEventBus eventBus, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            this._cache = cache;
            this._eventBus = eventBus;
            this._logger = loggerFactory.GetOrCreate("ThinkNet");
        }

        /// <summary>
        /// 从数据库中获取聚合。
        /// </summary>
        protected abstract T GetFromStorage<T>(object aggregateId)
            where T : class, IAggregateRoot;

        /// <summary>
        /// 查找聚合。如果不存在返回null，存在返回实例
        /// </summary>
        public TAggregateRoot Find<TAggregateRoot>(object id) where TAggregateRoot : class, IAggregateRoot
        {
            var aggregate = (TAggregateRoot)_cache.Get(typeof(TAggregateRoot), id);
            if (aggregate == null) {
                aggregate = this.GetFromStorage<TAggregateRoot>(id.ToString());
                if (aggregate != null) {
                    _cache.Set(aggregate, aggregate.Id);
                }
            }
            else {
                _logger.Info("Find the aggregate root {0} of id {1} from cache.",
                    typeof(TAggregateRoot).FullName, id.ToString());
            }

            return aggregate;
        }

        /// <summary>
        /// 根据标识id获得实体。
        /// </summary>
        public TAggregateRoot Get<TAggregateRoot>(object id) where TAggregateRoot : class, IAggregateRoot
        {
            var aggregate = this.Find<TAggregateRoot>(id);
            if (aggregate == null)
                throw new AggregateRootException(typeof(TAggregateRoot), id);

            return aggregate;
        }


        private void PublishEvents(IList<IEvent> events, string correlationId)
        {
            if (events.Count <= 0)
                return;

            //if (!string.IsNullOrEmpty(correlationId)) {
            //    events.Insert(0, new CommandHandled(correlationId, events.Select(@event => @event.Id).ToArray()));
            //}
            _eventBus.Publish(events);
        }

        /// <summary>
        /// 提交所有聚合的实现。
        /// </summary>
        protected abstract void DoCommit(IEnumerable<IAggregateRoot> addedAggregateRoots, IEnumerable<IAggregateRoot> modifiedAggregateRoots, IEnumerable<IAggregateRoot> deletedAggregateRoots);
        /// <summary>
        /// 提交所有聚合。
        /// </summary>
        public void Commit(IEnumerable<IAggregateRoot> addedAggregateRoots, IEnumerable<IAggregateRoot> modifiedAggregateRoots, IEnumerable<IAggregateRoot> deletedAggregateRoots, string correlationId)
        {
            bool added = false;
            bool modified = false;
            bool deleted = false;
            List<IAggregateRoot> trackingObjects = new List<IAggregateRoot>();
            if (addedAggregateRoots != null && addedAggregateRoots.Any()) {
                trackingObjects.AddRange(addedAggregateRoots);
                added = true;
            }
            if (modifiedAggregateRoots != null && modifiedAggregateRoots.Any()) {
                trackingObjects.AddRange(modifiedAggregateRoots);
                modified = true;
            }
            if (deletedAggregateRoots != null && deletedAggregateRoots.Any()) {
                trackingObjects.AddRange(deletedAggregateRoots);
                deleted = true;
            }

            if (trackingObjects.Count <= 0)
                return;

            this.DoCommit(addedAggregateRoots, modifiedAggregateRoots, deletedAggregateRoots);

            if (added) {
                addedAggregateRoots.ForEach(aggregateRoot => {
                    _cache.Set(aggregateRoot, aggregateRoot.Id);
                });
            }
            if (modified) {
                modifiedAggregateRoots.ForEach(aggregateRoot => {
                    _cache.Set(aggregateRoot, aggregateRoot.Id);
                });
            }
            if (deleted) {
                deletedAggregateRoots.ForEach(aggregateRoot => {
                    _cache.Remove(aggregateRoot.GetType(), aggregateRoot.Id);
                });
            }

            var pendingEvents = trackingObjects.OfType<IEventPublisher>().SelectMany(item => item.Events).ToList();

            this.PublishEvents(pendingEvents, correlationId);
        }
    }
}

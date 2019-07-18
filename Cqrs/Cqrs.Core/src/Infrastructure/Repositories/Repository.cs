using Cqrs.Database;
using Cqrs.Infrastructure.Logging;
using Cqrs.Infrastructure.Storage;

namespace Cqrs.Infrastructure.Repositories
{
    /// <summary>
    /// 仓储接口实现
    /// </summary>
    /// <typeparam name="TAggregateRoot">聚合类型</typeparam>
    public class Repository<TAggregateRoot> : IRepository<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot
    {
        private readonly IDataContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public Repository(IDataContext context)
            : this(context, DefaultMemoryCache.Instance, DefaultLoggerFactory.Instance.GetOrCreate("Cqrs"))
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public Repository(IDataContext context, IMemoryCache cache, ILogger logger)
        {
            this._context = context;
            this._cache = cache;
            this._logger = logger;
        }

        /// <summary>
        /// 数据上下文
        /// </summary>
        protected IDataContext DataContext
        {
            get { return this._context; }
        }
        /// <summary>
        /// 缓存程序
        /// </summary>
        protected IMemoryCache Cache
        {
            get { return this._cache; }
        }
        /// <summary>
        /// 日志程序
        /// </summary>
        protected ILogger Logger
        {
            get { return this._logger; }
        }

        /// <summary>
        /// 添加聚合根到仓储
        /// </summary>
        public virtual void Add(TAggregateRoot aggregateRoot)
        {
            _context.Save(aggregateRoot);

        }
        void IRepository<TAggregateRoot>.Add(TAggregateRoot aggregateRoot)
        {
            this.Add(aggregateRoot);

            _context.DataCommitted += (sender, args) => {
                _cache.Set(aggregateRoot, aggregateRoot.Id);
            };

            _logger.Info("the aggregate root {0} of id {1} is added the dbcontext.",
                    typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }
        /// <summary>
        /// 更新聚合到仓储
        /// </summary>
        public virtual void Update(TAggregateRoot aggregateRoot)
        {
            _context.Update(aggregateRoot);

        }
        void IRepository<TAggregateRoot>.Update(TAggregateRoot aggregateRoot)
        {
            this.Update(aggregateRoot);

            _context.DataCommitted += (sender, args) => {
                _cache.Set(aggregateRoot, aggregateRoot.Id);
            };

            _logger.Info("the aggregate root {0} of id {1} is updated the dbcontext.",
                    typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }
        /// <summary>
        /// 从仓储中移除聚合
        /// </summary>
        public virtual void Remove(TAggregateRoot aggregateRoot)
        {
            _context.Delete(aggregateRoot);
        }
        void IRepository<TAggregateRoot>.Remove(TAggregateRoot aggregateRoot)
        {
            this.Remove(aggregateRoot);

            _context.DataCommitted += (sender, args) => {
                _cache.Remove(typeof(TAggregateRoot), aggregateRoot.Id);
            };

            _logger.Info("updated the aggregate root {0} of id {1} in dbcontext.",
                   typeof(TAggregateRoot).FullName, aggregateRoot.Id.ToString());
        }


        /// <summary>
        /// 根据标识id获得实体
        /// </summary>
        public TAggregateRoot Get<TIdentify>(TIdentify id)
        {
            var aggregate = (this as IRepository<TAggregateRoot>).Find(id);
            if (aggregate == null)
                throw new AggregateRootException(typeof(TAggregateRoot), id);

            return aggregate;
        }

        /// <summary>
        /// 根据标识id获取聚合实例，如未找到则返回null
        /// </summary>
        public virtual TAggregateRoot Find<TIdentify>(TIdentify id)
        {
            return _context.Get<TAggregateRoot>(id);
        }
        TAggregateRoot IRepository<TAggregateRoot>.Find<TIdentify>(TIdentify id)
        {
            var aggregate = (TAggregateRoot)_cache.Get(typeof(TAggregateRoot), id);
            if (aggregate == null) {
                aggregate = this.Find(id);
                _logger.Info("find the aggregate root {0} of id {1} from storage.",
                    typeof(TAggregateRoot).FullName, id.ToString());
                if (aggregate != null) {
                    _cache.Set(aggregate, aggregate.Id);
                }
            }
            else {
                _logger.Info("find the aggregate root {0} of id {1} from cache.",
                    typeof(TAggregateRoot).FullName, id.ToString());
            }

            return aggregate;
        }
    }
}

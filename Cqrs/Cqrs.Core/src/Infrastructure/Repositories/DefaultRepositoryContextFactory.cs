using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Cqrs.Components;
using Cqrs.Database;
using Cqrs.Infrastructure.Logging;
using Cqrs.Infrastructure.Storage;
using Cqrs.Messaging;
using Cqrs.Messaging.Runtime;


namespace Cqrs.Infrastructure.Repositories
{
    /// <summary>
    /// <see cref="IRepositoryContextFactory"/> 的系统默认实现
    /// </summary>
    public class DefaultRepositoryContextFactory : IRepositoryContextFactory, IInitializer
    {
        class RepositoryContext : DisposableObject, IRepositoryContext
        {
            private readonly IDataContext _context;
            private readonly IEventBus _eventBus;
            private readonly IMemoryCache _cache;
            private readonly ILogger _logger;
            private readonly ConcurrentDictionary<Type, object> _repositories;

            public RepositoryContext(IDataContext context, IMemoryCache cache, IEventBus eventBus, ILogger logger)
            {
                this._repositories = new ConcurrentDictionary<Type, object>();
                this._context = context;
                this._cache = cache;
                this._eventBus = eventBus;
                this._logger = logger;
            }

            public void Commit()
            {
                this.Commit(string.Empty);
            }

            public void Commit(string correlationId)
            {
                _context.Commit();

                var events = _context.TrackingObjects.OfType<IEventPublisher>()
                    .SelectMany(item => item.Events).ToList();

                if (events.Count == 0)
                    return;

                if (string.IsNullOrWhiteSpace(correlationId)) {
                    _eventBus.Publish(events);
                }
                else {
                    _eventBus.Publish(new EventStream {
                        CommandId = correlationId,
                        Events = events
                    });
                }
                _logger.Info("publish all events. event ids: [{0}]", string.Join(",", events.Select(@event => @event.Id).ToArray()));

                
                //var eventIds = events.Select(@event => @event.Id).ToArray();
                //if (!string.IsNullOrEmpty(correlationId)) {
                //    events.Insert(0, new CommandHandled(correlationId, eventIds));
                //}
                //_eventBus.Publish(events);
            }

            private object CreateRepository(Type repositoryType)
            {
                var serviceType = GetRepositoryType(repositoryType);
                var constructor = serviceType.GetConstructor(new[] { typeof(IDataContext), typeof(IMemoryCache), typeof(ILogger) });
                if (constructor != null) {
                    return constructor.Invoke(new object[] { _context, _cache, _logger });
                }
                constructor = serviceType.GetConstructor(new[] { typeof(IDataContext) });
                if (constructor != null) {
                    return constructor.Invoke(new object[] { _context });
                }

                string errorMessage = string.Format("Type '{0}' must have a constructor with the following signature: .ctor(IDbContext) or .ctor(IDbContext,IMemoryCache,ILogger)", serviceType.FullName);
                _logger.Error(errorMessage);
                throw new InvalidCastException(errorMessage);
            }

            private Type GetRepositoryType(Type repositoryType)
            {
                Type implementationType = null;
                if (!repositoryMap.TryGetValue(repositoryType, out implementationType)) {
                    if (!TypeHelper.IsRepositoryInterfaceType(repositoryType)) {
                        string errorMessage = string.Format("The repository type '{0}' does not extend interface IRepository<>.", repositoryType.FullName);
                        _logger.Error(errorMessage);
                        throw new SystemException(errorMessage);
                    }

                    var aggregateRootType = repositoryType.GetGenericArguments().Single();
                    implementationType = typeof(Repository<>).MakeGenericType(aggregateRootType);
                    //repositoryMap[repositoryType] = implementationType;
                    repositoryMap.TryAdd(repositoryType, implementationType);
                }

                return implementationType;
            }

            public TRepository GetRepository<TRepository>() where TRepository : IRepository
            {
                return (TRepository)_repositories.GetOrAdd(typeof(TRepository), CreateRepository);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing) {
                    _repositories.Clear();
                    _context.Dispose();
                }
            }
        }


        private readonly IDataContextFactory _dbContextFactory;
        private readonly IEventBus _eventBus;
        private readonly IMemoryCache _cache;
        private readonly ILogger _logger;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DefaultRepositoryContextFactory(IDataContextFactory dbContextFactory, IEventBus eventBus, IMemoryCache cache, ILoggerFactory loggerFactory)
        {
            this._dbContextFactory = dbContextFactory;
            this._eventBus = eventBus;
            this._cache = cache;
            this._logger = loggerFactory.GetOrCreate("Cqrs");
        }

        /// <summary>
        /// 创建一个仓储上下文实例。
        /// </summary>
        public IRepositoryContext CreateRepositoryContext()
        {
            return new RepositoryContext(_dbContextFactory.CreateDataContext(), _cache, _eventBus, _logger);
        }

        /// <summary>
        /// 获取当前的仓储上下文
        /// </summary>
        public IRepositoryContext GetCurrentRepositoryContext()
        {
            var dbContext = _dbContextFactory.GetCurrentDataContext();

            return new RepositoryContext(dbContext, _cache, _eventBus, _logger);
        }


        internal static readonly ConcurrentDictionary<Type, Type> repositoryMap = new ConcurrentDictionary<Type, Type>();

        private void RegisterType(Type type)
        {
            type.GetInterfaces().Where(TypeHelper.IsRepositoryInterfaceType)
                .ForEach(repository => {
                    repositoryMap.TryAdd(repository, type);
                });
        }

        void IInitializer.Initialize(IContainer container, IEnumerable<Type> types)
        {
            //var types = assemblies.SelectMany(assembly => assembly.GetTypes());
            types.Where(TypeHelper.IsRepositoryType).ForEach(RegisterType);
        }
    }
}

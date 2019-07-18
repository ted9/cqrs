using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Cqrs.EventSourcing;
using Cqrs.EventSourcing.Streams;
using Cqrs.Infrastructure.Logging;
using Cqrs.Infrastructure.Serialization;
using Cqrs.Messaging;
using Cqrs.Messaging.Runtime;


namespace Cqrs.Infrastructure.Repositories
{
    /// <summary>
    /// <see cref="IEventSourcedRepository"/> 的默认实现类
    /// </summary>
    public class DefaultEventSourcedRepository : IEventSourcedRepository
    {
        private readonly IEventStore _eventStore;
        private readonly ITextSerializer _serializer;
        private readonly ISnapshotStore _snapshotStore;
        private readonly IMemoryCache _cache;
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DefaultEventSourcedRepository(IEventStore eventStore,
            ISnapshotStore snapshotStore,
            IMemoryCache cache,
            ITextSerializer serializer,
            ITypeCodeProvider typeCodeProvider,
            IEventBus eventBus,
            ILoggerFactory loggerFactory)
        {
            this._eventStore = eventStore;
            this._serializer = serializer;
            this._snapshotStore = snapshotStore;
            this._cache = cache;
            this._typeCodeProvider = typeCodeProvider;
            this._eventBus = eventBus;
            this._logger = loggerFactory.GetOrCreate("Cqrs");
        }


        /// <summary>
        /// 根据主键获取聚合根实例。
        /// </summary>
        public TAggregateRoot Get<TAggregateRoot>(object id) where TAggregateRoot : class, IEventSourced
        {
            var aggregate = this.Find<TAggregateRoot>(id);
            if (aggregate == null)
                throw new EventSourcedException(typeof(TAggregateRoot), id);

            return aggregate;
        }
        /// <summary>
        /// 根据主键获取聚合根实例。
        /// </summary>
        public TAggregateRoot Find<TAggregateRoot>(object id) where TAggregateRoot : class, IEventSourced
        {
            var aggregate = (TAggregateRoot)_cache.Get(typeof(TAggregateRoot), id);

            var aggregateId = id.ToString();
            if (aggregate == null) {
                aggregate = this.GetFromStorage<TAggregateRoot>(aggregateId);
                _logger.Info("find the aggregate root {0} of id {1} from storage.",
                    typeof(TAggregateRoot).FullName, aggregateId);
                if (aggregate != null) {
                    _cache.Set(aggregate, aggregate.Id);
                }
            }
            else {
                aggregate.SubscribeEvents();
                _logger.Info("find the aggregate root {0} of id {1} from cache.",
                    typeof(TAggregateRoot).FullName, aggregateId);
            }

            return aggregate;
        }


        /// <summary>
        /// 保存聚合事件。
        /// </summary>
        public void Save(IEventSourced aggregateRoot, string correlationId = null)
        {
            var events = aggregateRoot.GetEvents();

            if (events.IsEmpty())
                return;

            var aggregateRootType = aggregateRoot.GetType();
            int aggregateRootTypeCode = _typeCodeProvider.GetTypeCode(aggregateRootType);
            string aggregateRootId = aggregateRoot.Id.ToString();

            if (!_eventStore.IsExist(correlationId)) {
                var sourcedEvents = events.Select(@event => new EventData() {
                    AggregateRootId = aggregateRootId,
                    AggregateRootTypeCode = aggregateRootTypeCode,
                    Version = @event.Version,
                    CorrelationId = correlationId,
                    Payload = _serializer.Serialize(@event)
                }).ToArray();
                _eventStore.Append(sourcedEvents);

                _logger.Info("sourcing events persistent completed. aggregateRootId:{0},aggregateRootType:{1}.",
                    aggregateRootId, aggregateRootType.FullName);

                _cache.Set(aggregateRoot, aggregateRoot.Id);
            }
            else {
                events = _eventStore.FindAll(correlationId).Select(Deserialize);
                _logger.Info("the command generates events have been saved, load from storage. command id:{0}", correlationId);
            }

            _eventBus.Publish(new EventStream {
                AggregateRootId = aggregateRootId,
                AggregateRootTypeCode = aggregateRootTypeCode,
                CommandId = correlationId,
                StartVersion = events.Min(item => item.Version),
                EndVersion = events.Max(item => item.Version),
                Events = events.OfType<IEvent>().ToArray()
            });
            _logger.Info("publish all events. event ids: [{0}]", string.Join(",", events.Select(@event => @event.Id).ToArray()));

            if (_snapshotStore.StorageEnabled) {
                var snapshot = new SnapshotData(aggregateRootTypeCode, aggregateRootId) {
                    Data = _serializer.Serialize(aggregateRoot),
                    Version = aggregateRoot.Version
                };

                _snapshotStore.Save(snapshot).ContinueWith(task => {
                    if (task.Status == TaskStatus.Faulted) {
                        _logger.Error(task.Exception, "snapshot persistent failed. aggregateRootId:{0},aggregateRootType:{1},version:{2}.", aggregateRootId, aggregateRootType.FullName, aggregateRoot.Version);
                    }
                });
                //Task.Factory.StartNew(() => {
                //    try {
                //        _snapshotStore.Save(snapshot);
                //    }
                //    catch (Exception ex) {
                //        _logger.Error(ex, "snapshot persistent failed. aggregateRootId:{0},aggregateRootType:{1},version:{2}.", aggregateRootId, aggregateRootType.FullName, aggregateRoot.Version);
                //    }
                //});
            }
        }


        private T GetFromStorage<T>(string aggregateId)
            where T : class, IEventSourced
        {
            T aggregateRoot;

            int aggregateTypeCode = _typeCodeProvider.GetTypeCode(typeof(T));

            if (!TryGetFromSnapshot(aggregateId, aggregateTypeCode, out aggregateRoot)) {
                aggregateRoot = (T)FormatterServices.GetUninitializedObject(typeof(T));
            }
            aggregateRoot.SubscribeEvents();


            var events = QueryEvents(aggregateId, aggregateTypeCode, aggregateRoot.Version);
            aggregateRoot.LoadFrom(events);

            return aggregateRoot;
        }


        private bool TryGetFromSnapshot<T>(string aggregateRootId, int aggregateRootType, out T aggregateRoot)
            where T : class, IEventSourced
        {
            var snapshot = _snapshotStore.GetLastestSnapshot(aggregateRootId, aggregateRootType).ContinueWith(task => {
                if (task.Status == TaskStatus.Faulted) {
                    _logger.Error(task.Exception, "get the latest aggregateRoot from snapshotStore failed. aggregateRootId:{0},aggregateRootType:{1}.", aggregateRootId, typeof(T).FullName);
                    return null;
                }

                return task.Result;
            }).Result;

            if (snapshot == null) {
                aggregateRoot = null;
                return false;
            }

            aggregateRoot = _serializer.Deserialize<T>(snapshot.Data);

            return true;
        }
        private IEnumerable<IVersionedEvent> QueryEvents(string aggregateId, int aggregateTypeCode, int minVersion)
        {
            var events = _eventStore.FindAll(aggregateId, aggregateTypeCode, minVersion, int.MaxValue);

            return events.Select(Deserialize);

        }
        /// <summary>
        /// 删除聚合相关的事件。
        /// </summary>
        public void Remove(IEventSourced aggregate)
        {
            this.Remove(aggregate.GetType(), aggregate.Id);
        }
        /// <summary>
        /// 删除聚合相关的事件。
        /// </summary>
        public void Remove(Type aggregateType, object aggregateId)
        {
            int aggregateRootTypeCode = _typeCodeProvider.GetTypeCode(aggregateType);
            var aggregateRootId = aggregateId.ToString();

            _snapshotStore.Remove(aggregateRootId, aggregateRootTypeCode);
            _eventStore.RemoveAll(aggregateRootId, aggregateRootTypeCode);
        }

        private IVersionedEvent Deserialize(EventData @event)
        {
            return _serializer.Deserialize<IVersionedEvent>(@event.Payload);
            //using (var reader = new StringReader(@event.Payload)) {
            //    return (IVersionedEvent)_serializer.Deserialize(reader);
            //}
        }
    }
}

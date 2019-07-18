using System.Collections.Concurrent;


namespace Cqrs.Messaging.Runtime
{
    public class EventPublishedVersionInMemory : IEventPublishedVersionStore
    {
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, int>> _versionCache;

        public EventPublishedVersionInMemory()
        {
            this._versionCache = new ConcurrentDictionary<int, ConcurrentDictionary<string, int>>();
        }

        public virtual void WriteFirstVersion(EventPublishedVersionData versionData)
        {
            this.UpdatePublishedVersion(versionData);
        }

        public virtual void UpdatePublishedVersion(EventPublishedVersionData versionData)
        {
            _versionCache.GetOrAdd(versionData.AggregateRootTypeCode, aggregateRootTypeCode => new ConcurrentDictionary<string, int>())
                .AddOrUpdate(versionData.AggregateRootId,
                    aggregateRootId => versionData.Version,
                    (aggregateRootId, version) => {
                        if (version > versionData.Version)
                            return version;

                        return versionData.Version;
                    });
        }

        public virtual int GetPublishedVersion(int aggregateRootTypeCode, string aggregateRootId)
        {
            int version;
            if (_versionCache.GetOrAdd(aggregateRootTypeCode, typeCode => new ConcurrentDictionary<string, int>())
                .TryGetValue(aggregateRootId, out version)) {
                return version;
            }

            return 0;
        }
    }
}

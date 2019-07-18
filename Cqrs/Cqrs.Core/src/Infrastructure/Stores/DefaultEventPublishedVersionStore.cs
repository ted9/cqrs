using System.Linq;
using Cqrs.Messaging.Runtime;
using Cqrs.Infrastructure.Storage;

namespace Cqrs.Infrastructure.Stores
{
    public class DefaultEventPublishedVersionStore : EventPublishedVersionInMemory
    {
        private readonly IDataContextFactory _contextFactory;
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public DefaultEventPublishedVersionStore(IDataContextFactory contextFactory)
        {
            this._contextFactory = contextFactory;
        }

        public override int GetPublishedVersion(int aggregateRootTypeCode, string aggregateRootId)
        {
            var version = base.GetPublishedVersion(aggregateRootTypeCode, aggregateRootId);

            if (version <= 0) {
                using (var context = _contextFactory.CreateDataContext()) {
                    var data = context.Get<EventPublishedVersionData>(aggregateRootTypeCode, aggregateRootId);
                    if (data != null) {
                        version = data.Version;
                    }
                }
            }

            return version;
        }


        public override void WriteFirstVersion(EventPublishedVersionData versionData)
        {
            base.WriteFirstVersion(versionData);

            using (var context = _contextFactory.CreateDataContext()) {
                var created = context.CreateQuery<EventPublishedVersionData>()
                    .Any(p => p.AggregateRootId == versionData.AggregateRootId &&
                        p.AggregateRootTypeCode == versionData.AggregateRootTypeCode);

                if (!created) {
                    context.Save(versionData);
                    context.Commit();
                }
            }
        }

        public override void UpdatePublishedVersion(EventPublishedVersionData versionData)
        {
            base.UpdatePublishedVersion(versionData);

            using (var context = _contextFactory.CreateDataContext()) {
                context.Update(versionData);
                context.Commit();
            }
        }
    }
}

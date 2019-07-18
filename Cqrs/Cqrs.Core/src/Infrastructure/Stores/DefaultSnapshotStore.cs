using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Cqrs.EventSourcing.Streams;
using Cqrs.Infrastructure.Storage;


namespace Cqrs.Infrastructure.Stores
{
    public class DefaultSnapshotStore : EmptySnapshotStore
    {
        private readonly IDataContextFactory _dbContextFactory;
        private readonly bool _persistent;
        public DefaultSnapshotStore(IDataContextFactory dbContextFactory, ITypeCodeProvider typeCodeProvider)
            : base(typeCodeProvider)
        {
            this._dbContextFactory = dbContextFactory;
            this._persistent = ConfigurationManager.AppSettings["thinkcfg.snapshot_storage"].Safe("false").ToBoolean();
        }

        /// <summary>
        /// 是否启用快照存储
        /// </summary>
        public override bool StorageEnabled
        {
            get { return _persistent; }
        }
        
        /// <summary>
        /// 获取最新的快照
        /// </summary>
        public override Task<SnapshotData> GetLastestSnapshot(string aggregateRootId, int aggregateRootTypeCode)
        {
            if (!_persistent) return base.GetLastestSnapshot(aggregateRootId, aggregateRootTypeCode);

            return Task.Factory.StartNew(() => {
                using (var context = _dbContextFactory.CreateDataContext()) {
                    return context.CreateQuery<SnapshotData>()
                        .Where(snapshot => snapshot.AggregateRootId == aggregateRootId &&
                            snapshot.AggregateRootTypeCode == aggregateRootTypeCode)
                        .OrderByDescending(snapshot => snapshot.Version)
                        .FirstOrDefault();
                }
            });
        }

        /// <summary>
        /// 删除该聚合下的所有快照
        /// </summary>
        public override Task Remove(string aggregateRootId, int aggregateRootTypeCode)
        {
            if (!_persistent) return base.Remove(aggregateRootId, aggregateRootTypeCode);

            return Task.Factory.StartNew(() => {
                using (var context = _dbContextFactory.CreateDataContext()) {
                    context.CreateQuery<SnapshotData>()
                        .Where(snapshot => snapshot.AggregateRootId == aggregateRootId &&
                            snapshot.AggregateRootTypeCode == aggregateRootTypeCode)
                        .ToList()
                        .ForEach(context.Delete);
                    context.Commit();
                }
            });
        }

        /// <summary>
        /// 保存快照
        /// </summary>
        public override Task Save(SnapshotData snapshot)
        {
            if (!_persistent) return base.Save(snapshot);

            var triggered = base.GetTriggeredVersion(snapshot.AggregateRootTypeCode);

            if (snapshot.Version % triggered != 0) {
                return base.Save(snapshot);
            }

            return Task.Factory.StartNew(() => {
                using (var context = _dbContextFactory.CreateDataContext()) {
                    bool exist = context.CreateQuery<SnapshotData>()
                        .Any(entity => entity.AggregateRootId == snapshot.AggregateRootId &&
                            entity.AggregateRootTypeCode == snapshot.AggregateRootTypeCode);
                    if (exist) {
                        context.Update(snapshot);
                    }
                    else {
                        context.Save(snapshot);
                    }
                    context.Commit();
                }
            });
        }
    }
}

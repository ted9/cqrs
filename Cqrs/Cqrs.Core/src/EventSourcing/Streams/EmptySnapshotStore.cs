using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Cqrs.Infrastructure;


namespace Cqrs.EventSourcing.Streams
{
    /// <summary>Represents a snapshot store that always not store any snapshot.
    /// </summary>
    public class EmptySnapshotStore : ISnapshotStore
    {
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly ConcurrentDictionary<int, int> _snapshotVersion;
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public EmptySnapshotStore(ITypeCodeProvider typeCodeProvider)
        {
            this._typeCodeProvider = typeCodeProvider;
            this._snapshotVersion = new ConcurrentDictionary<int, int>();
        }

        private int GetDefineVersion(int aggregateTypeCode)
        {
            var aggregateType = _typeCodeProvider.GetType(aggregateTypeCode);

            if (!aggregateType.IsDefined<SnapshotPolicyAttribute>(false)) {
                return 20;
            }

            return aggregateType.GetAttribute<SnapshotPolicyAttribute>(false).TriggeredVersion;
        }

        /// <summary>
        /// 获取触发保存快照的间隔版本号。
        /// </summary>
        protected int GetTriggeredVersion(int aggregateRootTypeCode)
        {
            return _snapshotVersion.GetOrAdd(aggregateRootTypeCode, GetDefineVersion);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        public virtual Task Save(SnapshotData snapshot)
        {
            return Task.Factory.StartNew(() => { });
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        public virtual Task Remove(string aggregateRootId, int aggregateRootTypeCode)
        {
            return Task.Factory.StartNew(() => { });
        }

        /// <summary>
        /// Always return null.
        /// </summary>
        public virtual Task<SnapshotData> GetLastestSnapshot(string aggregateRootId, int aggregateRootTypeCode)
        {
            return Task.Factory.StartNew<SnapshotData>(() => null);
        }

        /// <summary>
        /// Always return false.
        /// </summary>
        public virtual bool StorageEnabled
        {
            get { return false; }
        }
    }
}

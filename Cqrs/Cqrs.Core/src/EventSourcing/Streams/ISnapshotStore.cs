using System.Threading.Tasks;
using Cqrs.Components;
using Cqrs.Infrastructure.Stores;

namespace Cqrs.EventSourcing.Streams
{
    /// <summary>
    /// 存储快照
    /// </summary>
    [RequiredComponent(typeof(DefaultSnapshotStore))]
    public interface ISnapshotStore
    {
        /// <summary>
        /// 是否启用存储。
        /// </summary>
        bool StorageEnabled { get; } 

        /// <summary>
        /// 存储给定的快照。
        /// </summary>
        Task Save(SnapshotData snapshot);
        /// <summary>
        /// 从存储中删除快照。
        /// </summary>
        Task Remove(string aggregateRootId, int aggregateRootTypeCode);
        /// <summary>
        /// 获取指定聚合根的最新快照。
        /// </summary>
        Task<SnapshotData> GetLastestSnapshot(string aggregateRootId, int aggregateRootTypeCode);
    }
}

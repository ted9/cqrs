using System.Collections.Generic;
using Cqrs.Components;
using Cqrs.Infrastructure.Stores;

namespace Cqrs.EventSourcing.Streams
{
    /// <summary>
    /// 事件存储。
    /// </summary>
    [RequiredComponent(typeof(DefaultEventStore))]
    public interface IEventStore
    {
        /// <summary>
        /// 添加溯源事件。
        /// </summary>
        void Append(IEnumerable<EventData> events);

        /// <summary>
        /// 判断该命令下是否存在相关事件。
        /// </summary>
        bool IsExist(string commandId);

        /// <summary>
        /// 查询该命令下的溯源事件。
        /// </summary>
        IEnumerable<EventData> FindAll(string commandId);
        /// <summary>
        /// 查询聚合的溯源事件。
        /// </summary>
        IEnumerable<EventData> FindAll(string aggregateRootId, int aggregateRootTypeCode, int minVersion, int maxVersion);

        /// <summary>
        /// 移除该聚合的溯源事件。
        /// </summary>
        void RemoveAll(string aggregateRootId, int aggregateRootTypeCode);
    }
}

using Cqrs.Components;
using Cqrs.Infrastructure.Stores;

namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// 表示一个存储器用来存储聚合事件的发布版本号。
    /// </summary>
    [RequiredComponent(typeof(DefaultEventPublishedVersionStore))]
    public interface IEventPublishedVersionStore
    {
        ///// <summary>
        ///// 更新版本号
        ///// </summary>
        //void UpdatePublishedVersion(int aggregateRootTypeCode, string aggregateRootId, int publishedVersion);
        /// <summary>
        /// 获取已发布的版本号
        /// </summary>
        int GetPublishedVersion(int aggregateRootTypeCode, string aggregateRootId);
        /// <summary>
        /// 写入首次版本号
        /// </summary>
        void WriteFirstVersion(EventPublishedVersionData versionData);
        /// <summary>
        /// 更新版本号
        /// </summary>
        void UpdatePublishedVersion(EventPublishedVersionData versionData);
        ///// <summary>
        ///// 获取已发布的版本号
        ///// </summary>
        //int GetPublishedVersion(EventPublishedVersionData versionData);
    }
}

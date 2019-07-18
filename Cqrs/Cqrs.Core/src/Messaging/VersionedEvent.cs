using System;


namespace Cqrs.Messaging
{
    /// <summary>
    /// 实现有序事件接口的抽象类
    /// </summary>
    [Serializable]
    public abstract class VersionedEvent<TSourceId> : Event<TSourceId>, IVersionedEvent
    {
        protected VersionedEvent()
            : this(null)
        { }

        protected VersionedEvent(string eventId)
            : base(eventId, default(TSourceId))
        { }

        /// <summary>
        /// 当前事件版本号
        /// </summary>
        public int Version { get; internal set; }

        /// <summary>
        /// 输出字符串信息
        /// </summary>
        public override string ToString()
        {
            return string.Concat(this.GetType().Name, "|", this.SourceId, "|", this.Version);
        }
    }
}

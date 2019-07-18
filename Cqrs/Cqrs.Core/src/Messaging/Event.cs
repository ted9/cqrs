using System;

namespace Cqrs.Messaging
{
    /// <summary>
    /// 实现 <see cref="IEvent"/> 的抽象类
    /// </summary>
    [Serializable]
    public abstract class Event : Message, IEvent, IRetry
    {
        /// <summary>
        /// 重试次数。
        /// </summary>
        protected int RetryTimes { get; set; } 

        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected Event()
            : this(null)
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Event(string id)
            : base(id)
        {
            this.RetryTimes = 5;
        }

        /// <summary>
        /// 获取源标识的字符串形式
        /// </summary>
        protected virtual string GetSourceStringId()
        {
            return null;
        }

        int IRetry.RetryTimes
        {
            get { return this.RetryTimes; }
        }

        string IEvent.SourceId
        {
            get { return this.GetSourceStringId(); }
        }
    }

    /// <summary>
    /// Represents an abstract sourced event.
    /// </summary>
    [Serializable]
    public abstract class Event<TSourceId> : Event
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        protected Event()
        { }
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected Event(TSourceId sourceId)
            : this(null, sourceId)
        { }
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        protected Event(string eventId, TSourceId sourceId)
            : base(eventId)
        {
            this.SourceId = sourceId;
        }

        /// <summary>
        /// 事件来源的标识id
        /// </summary>
        public TSourceId SourceId { get; internal set; }


        protected override string GetSourceStringId()
        {
           return this.SourceId.ToString(); 
        }

        /// <summary>
        /// Returns the source id as the key.
        /// </summary>
        protected override string GetRoutingKey()
        {
            return this.GetSourceStringId().Safe(base.Id);
        }
    }
}

using System;
using System.Collections.Generic;

using Cqrs.Infrastructure;
using Cqrs.Messaging;


namespace Cqrs.Database
{
    /// <summary>
    /// 实现 <see cref="IAggregateRoot"/> 的抽象类
    /// </summary>
    [Serializable]
    public abstract class AggregateRoot<TIdentify> : Entity<TIdentify>, IAggregateRoot, IEventPublisher
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AggregateRoot()
            : this(default(TIdentify))
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        protected AggregateRoot(TIdentify id)
            : base(id)
        { }

        [NonSerialized]
        private IList<IEvent> uncommittedEvents;
        /// <summary>
        /// 筹集事件
        /// </summary>
        protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : Event<TIdentify>
        {
            Ensure.NotNull(@event, "@event");

            @event.SourceId = this.Id;
            if (uncommittedEvents == null) {
                uncommittedEvents = new List<IEvent>();
            }
            uncommittedEvents.Add(@event);
        }

        /// <summary>
        /// 获取未发布的事件集合。
        /// </summary>
        protected IEnumerable<IEvent> GetUnpublishedEvents()
        {
            return this.uncommittedEvents ?? new List<IEvent>();
        }
        /// <summary>
        /// 清除事件。
        /// </summary>
        protected void ClearEvents()
        {
            if (uncommittedEvents != null) {
                uncommittedEvents.Clear();
                uncommittedEvents = null;
            }
        }

        IEnumerable<IEvent> IEventPublisher.Events
        {
            get { return this.GetUnpublishedEvents(); }
        }
    }
}

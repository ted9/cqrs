using System;
using System.Collections.Generic;

using Cqrs.Infrastructure;


namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// <see cref="IEventBus"/> 的实现
    /// </summary>
    public class DefaultEventBus : AbstractBus, IEventBus
    {
        private readonly IEventProcessor _eventProcessor;
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public DefaultEventBus(IEventProcessor eventProcessor)
        {
            this._eventProcessor = eventProcessor;
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish(IEvent @event)
        {
            this.Publish(new IEvent[] { @event });
        }

        /// <summary>
        /// 发布一组事件
        /// </summary>
        public void Publish(IEnumerable<IEvent> events)
        {
            if (events.IsEmpty())
                return;

            events.ForEach(@event => Ensure.NotNull(@event, "@event"));

            _eventProcessor.Receive(events);
        }

        protected override bool SearchMatchType(Type type)
        {
            return TypeHelper.IsEvent(type);
        }
    }
}

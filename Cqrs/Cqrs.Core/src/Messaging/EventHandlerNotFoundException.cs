using System;

namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示一个当找不到事件处理程序的异常
    /// </summary>
    [Serializable]
    public class EventHandlerNotFoundException : CqrsException
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        public EventHandlerNotFoundException(Type eventType)
            : base(string.Format("Event Handler not found for {0}.", eventType.FullName))
        { }
    }
}

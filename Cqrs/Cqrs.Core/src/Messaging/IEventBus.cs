using System.Collections.Generic;
using Cqrs.Components;
using Cqrs.Messaging.Runtime;


namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示继承该接口的是事件总线
    /// </summary>
    [RequiredComponent(typeof(DefaultEventBus))]
    public interface IEventBus
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        void Publish(IEvent @event);

        /// <summary>
        /// 发布一组事件
        /// </summary>
        void Publish(IEnumerable<IEvent> events);
    }   
}

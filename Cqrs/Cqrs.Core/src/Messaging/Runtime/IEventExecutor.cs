using Cqrs.Components;


namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// 继承该接口的是一个事件执行器
    /// </summary>
    [RequiredComponent(typeof(DefaultEventExecutor))]
    public interface IEventExecutor : IMessageExecutor<IEvent>
    { }
}

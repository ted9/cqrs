using Cqrs.Components;

namespace Cqrs.Messaging.Runtime
{
    [RequiredComponent(typeof(DefaultEventProcessor))]
    public interface IEventProcessor : IMessageProcessor<IEvent>
    { }
}

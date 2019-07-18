using Cqrs.Components;

namespace Cqrs.Messaging.Runtime
{
    [RequiredComponent(typeof(DefaultCommandProcessor))]
    public interface ICommandProcessor : IMessageProcessor<ICommand>
    { }
}

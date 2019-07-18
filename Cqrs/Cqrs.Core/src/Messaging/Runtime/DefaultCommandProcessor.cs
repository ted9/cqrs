using Cqrs.Infrastructure.Serialization;
using Cqrs.Messaging.Queue;

namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// 命令处理器
    /// </summary>
    public class DefaultCommandProcessor : MessageProcessor<ICommand>, ICommandProcessor
    {
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public DefaultCommandProcessor(IMessageQueueFactory queueFactory, ICommandExecutor commandExecutor,
            IMessageStore messageStore, ITextSerializer serializer)
            : base("Command", queueFactory.CreateGroup(10), commandExecutor, messageStore, serializer)
        { }
    }
}

using Cqrs.Infrastructure.Serialization;
using Cqrs.Messaging.Queue;

namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// 事件任务处理器
    /// </summary>
    public class DefaultEventProcessor :  MessageProcessor<IEvent>, IEventProcessor
    {
       /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public DefaultEventProcessor(IMessageQueueFactory queueFactory, IEventExecutor eventExecutor,
            IMessageStore messageStore, ITextSerializer serializer)
            : base("Event", queueFactory.CreateGroup(10), eventExecutor, messageStore, serializer)
        { }   
    }
}

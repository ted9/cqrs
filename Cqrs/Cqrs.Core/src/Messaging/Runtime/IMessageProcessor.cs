using System.Collections.Generic;

namespace Cqrs.Messaging.Runtime
{
    public interface IMessageProcessor<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// 消息处理器名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 接收消息
        /// </summary>
        void Receive(IEnumerable<TMessage> messages);
    }
}


namespace Cqrs.Messaging.Queue
{
    /// <summary>
    /// 表示这是一个消息队列。
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// 将消息进队列。
        /// </summary>
        void Enqueue(IMessage message);
        /// <summary>
        /// 取出消息。
        /// </summary>
        IMessage Dequeue();
        /// <summary>
        /// 通知消息已处理。
        /// </summary>
        bool Ack();
    }
}


namespace Cqrs.Messaging.Runtime
{
    public interface IMessageExecutor<TMessage> where TMessage : class, IMessage
    {
        /// <summary>
        /// 执行消息
        /// </summary>
        void Execute(TMessage message);
    }
}

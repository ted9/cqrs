
namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示这是一个消息处理程序的拦截器。
    /// </summary>
    public interface IInterceptor<TMessage>
        where TMessage : class, IMessage
    {
        /// <summary>
        /// 在处理程序前执行
        /// </summary>
        void OnExecuting(TMessage message);
        /// <summary>
        /// 在处理程序后执行
        /// </summary>
        void OnExecuted(TMessage message);
    }
}

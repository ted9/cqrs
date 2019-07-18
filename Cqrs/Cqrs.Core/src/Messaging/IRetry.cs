
namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示继承该接口的类型是一个重试操作。
    /// </summary>
    public interface IRetry
    {
        /// <summary>
        /// 重试次数。
        /// </summary>
        int RetryTimes { get; }
    }
}

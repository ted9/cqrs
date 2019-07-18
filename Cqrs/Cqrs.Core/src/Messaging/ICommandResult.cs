
namespace Cqrs.Messaging
{
    public interface ICommandResult
    {
        /// <summary>
        /// 消息处理状态。
        /// </summary>
        CommandStatus Status { get; }
        /// <summary>
        /// Represents the unique identifier of the command.
        /// </summary>
        string CommandId { get; }
    }
}

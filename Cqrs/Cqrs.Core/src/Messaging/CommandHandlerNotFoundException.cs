using System;

namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示一个当找不到命令处理程序的异常
    /// </summary>
    [Serializable]
    public class CommandHandlerNotFoundException : CqrsException
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        public CommandHandlerNotFoundException(Type commandType)
            : base(string.Format("Command Handler not found for {0}.", commandType.FullName))
        { }
    }
}

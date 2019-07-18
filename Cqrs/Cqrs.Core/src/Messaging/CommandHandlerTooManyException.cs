using System;

namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示一个存在多个命令处理程序的异常
    /// </summary>
    public class CommandHandlerTooManyException : CqrsException
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        public CommandHandlerTooManyException(Type commandType)
            : base(string.Format("Found more than one command handler, commandType:{0}.", commandType.FullName)) 
        { }
    }
}

using System.Collections.Generic;
using Cqrs.Components;
using Cqrs.Messaging.Runtime;


namespace Cqrs.Messaging
{
    /// <summary>
    /// 表示继承该接口的是命令总线
    /// </summary>
    [RequiredComponent(typeof(DefaultCommandBus))]
    public interface ICommandBus
    {
        /// <summary>
        /// 发送命令
        /// </summary>
        void Send(ICommand command);
        /// <summary>
        /// 发送一组命令
        /// </summary>
        void Send(IEnumerable<ICommand> commands);
    }
}

using Cqrs.Components;

namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// 继承该接口的是一个命令执行器
    /// </summary>
    [RequiredComponent(typeof(DefaultCommandExecutor))]
    public interface ICommandExecutor : IMessageExecutor<ICommand>
    { }
}

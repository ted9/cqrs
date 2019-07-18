using System;
using System.Collections.Generic;
using System.Reflection;

using Cqrs.Components;
using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Logging;


namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// <see cref="ICommandBus"/> 的实现。
    /// </summary>
    public class DefaultCommandBus : AbstractBus, ICommandBus
    {
        private readonly ICommandProcessor _commandProcessor;
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public DefaultCommandBus(ICommandProcessor commandProcessor)
        {
            this._commandProcessor = commandProcessor;
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        public void Send(ICommand command)
        {
            this.Send(new ICommand[] { command });
        }
        /// <summary>
        /// 发送一组命令
        /// </summary>
        public void Send(IEnumerable<ICommand> commands)
        {
            if (commands.IsEmpty())
                return;

            commands.ForEach(command => Ensure.NotNull(command, "command"));

            _commandProcessor.Receive(commands);
        }


        protected override bool SearchMatchType(Type type)
        {
            return TypeHelper.IsCommand(type);
        }
    }
}

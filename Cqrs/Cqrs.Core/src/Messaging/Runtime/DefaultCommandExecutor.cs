using System;
using System.Collections.Generic;
using System.Linq;

using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Logging;
using Cqrs.Messaging.Handling;


namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// <see cref="ICommandExecutor"/> 的默认实现。
    /// </summary>
    public class DefaultCommandExecutor : MessageExecutor<ICommand>, ICommandExecutor
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DefaultCommandExecutor(IHandlerProvider handerProvider,
            IHandlerRecordStore handlerStore,
            ITypeCodeProvider typeCodeProvider,
            ILoggerFactory loggerFactory)
            : base(handerProvider, handlerStore, typeCodeProvider, loggerFactory)
        { }


        /// <summary>
        /// 执行命令
        /// </summary>
        protected override void TryExecute(Type commandType, ICommand command, IEnumerable<IProxyHandler> commandHandlers, ILogger logger)
        {
            if (commandHandlers.IsEmpty()) {
                var exception = new CommandHandlerNotFoundException(commandType);
                logger.Fatal(exception);

                throw exception;
            }

            if (commandHandlers.Count() > 1) {
                var exception = new CommandHandlerTooManyException(commandType);
                logger.Fatal(exception);

                throw exception;
            }

            ProcessHandler(commandType, command, commandHandlers.First());
        }
    }
}

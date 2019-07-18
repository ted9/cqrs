using System;
using System.Collections.Generic;

using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Logging;
using Cqrs.Messaging.Handling;


namespace Cqrs.Messaging.Runtime
{
    /// <summary>
    /// <see cref="IEventExecutor"/> 的默认实现。
    /// </summary>
    public class DefaultEventExecutor : MessageExecutor<IEvent>, IEventExecutor
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DefaultEventExecutor(IHandlerProvider handlerProvider,
            IHandlerRecordStore handlerStore,
            ITypeCodeProvider typeCodeProvider,
            ILoggerFactory loggerFactory)
            : base(handlerProvider, handlerStore, typeCodeProvider, loggerFactory)
        { }

        /// <summary>
        /// 执行事件。
        /// </summary>
        protected override void TryExecute(Type eventType, IEvent @event, IEnumerable<IProxyHandler> eventHandlers, ILogger logger)
        {
            if (eventHandlers.IsEmpty()) {
                var exception = new EventHandlerNotFoundException(eventType);
                logger.Fatal(exception);

                throw exception;
            }

            foreach (var eventHandler in eventHandlers) {
                ProcessHandler(eventType, @event, eventHandler);
            }
        }
    }
}

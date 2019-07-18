using System;
using System.Collections.Generic;

using Cqrs.Components;
using Cqrs.Infrastructure;
using Cqrs.Infrastructure.Logging;
using Cqrs.Messaging.Handling;


namespace Cqrs.Messaging.Runtime
{
    public abstract class MessageExecutor<TMessage>
        where TMessage : class, IMessage
    {
        private readonly ITypeCodeProvider _typeCodeProvider;
        private readonly IHandlerRecordStore _handlerStore;
        private readonly IHandlerProvider _handlerProvider;
        private readonly ILogger _logger;

        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public MessageExecutor(IHandlerProvider handlerProvider,
            IHandlerRecordStore handlerStore,
            ITypeCodeProvider typeCodeProvider,
            ILoggerFactory loggerFactory)
        {
            this._handlerProvider = handlerProvider;
            this._handlerStore = handlerStore;
            this._typeCodeProvider = typeCodeProvider;
            this._logger = loggerFactory.GetOrCreate("Cqrs");
        }

        /// <summary>
        /// 尝试执行消息处理结果。
        /// </summary>
        protected abstract void TryExecute(Type messageType, TMessage message, IEnumerable<IProxyHandler> messageHandlers, ILogger logger);

        /// <summary>
        /// 处理当前消息。
        /// </summary>
        protected void ProcessHandler(Type messageType, IMessage message, IProxyHandler messageHandler)
        {
            var messageHandlerType = messageHandler.GetInnerHandler().GetType();
            

            try {
                if (message is EventStream) {
                    messageHandler.Handle(message);
                }
                else {
                    var messageHandlerTypeCode = _typeCodeProvider.GetTypeCode(messageHandlerType);
                    var messageTypeCode = _typeCodeProvider.GetTypeCode(messageType);

                    var messageHandlerInfo = new HandlerRecordData(message.Id, messageHandlerTypeCode, messageTypeCode);

                    if (!_handlerStore.IsHandlerInfoExist(messageHandlerInfo)) {
                        messageHandler.Handle(message);

                        _handlerStore.AddHandlerInfo(messageHandlerInfo);
                    }

                    _logger.Debug("Handle message success. messageHandlerType:{0}, messageType:{1}, messageId:{2}",
                    messageHandlerType.FullName, messageType.FullName, message.Id);
                }                
            }
            catch (Exception ex) {
                if (!(message is EventStream)) {
                    string errorMessage = string.Format("Exception raised when {0} handling {1}. message info:{2}.",
                        messageHandlerType.FullName, messageType.FullName, message.ToString());
                    _logger.Error(ex, errorMessage);
                }

                throw ex;
            }
            finally {
                if (messageHandler is IDisposable) {
                    ((IDisposable)messageHandler).Dispose();
                }
            }
        }

        /// <summary>
        /// 执行消息
        /// </summary>
        public void Execute(TMessage message)
        {
            var messageType = message.GetType();

            this.TryExecute(messageType, message, _handlerProvider.GetHandlers(messageType), _logger);
        }
    }
}

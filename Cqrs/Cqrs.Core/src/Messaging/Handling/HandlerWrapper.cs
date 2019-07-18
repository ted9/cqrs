using System;
using Cqrs.Infrastructure;

namespace Cqrs.Messaging.Handling
{
    public class HandlerWrapper<T> : DisposableObject, IProxyHandler
        where T : class, IMessage
    {
        private readonly IHandler<T> _handler;
        /// <summary>
        /// Parameterized Constructor.
        /// </summary>
        public HandlerWrapper(IHandler<T> handler)
        {
            this._handler = handler;
        }

        /// <summary>
        /// Handles the given message with the provided context.
        /// </summary>
        public void Handle(T message)
        {
            var interceptor = _handler as IInterceptor<T>;

            if (interceptor != null)
                interceptor.OnExecuting(message);

            _handler.Handle(message);

            if (interceptor != null)
                interceptor.OnExecuted(message);
        }

        /// <summary>
        /// dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _handler is IDisposable) {
                ((IDisposable)_handler).Dispose();
            }
        }

        void IProxyHandler.Handle(IMessage message)
        {
            this.Handle(message as T);
        }

        object IProxyHandler.GetInnerHandler()
        {
            return _handler;
        }
    }
}

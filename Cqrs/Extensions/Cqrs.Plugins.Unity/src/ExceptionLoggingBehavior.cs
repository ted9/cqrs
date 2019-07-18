using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity.InterceptionExtension;
using Cqrs.Infrastructure.Logging;

namespace Cqrs.Plugins.Unity
{
    public class ExceptionLoggingBehavior : IInterceptionBehavior
    {
        private readonly ILogger _logger;
        public ExceptionLoggingBehavior(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.GetOrCreate("Cqrs");
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public virtual IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var methodReturn = getNext().Invoke(input, getNext);
            if (methodReturn.Exception != null) {
                _logger.Error(methodReturn.Exception);
            }
            return methodReturn;
        }
        public bool WillExecute
        {
            get { return true; }
        }
    }
}

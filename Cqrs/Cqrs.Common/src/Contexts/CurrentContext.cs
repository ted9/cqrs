using System;
using System.Collections;

namespace Cqrs.Contexts
{
    public abstract class CurrentContext : ICurrentContext
    {
        private readonly IContextManager _contextManager;

        protected CurrentContext(IContextManager contextManager)
        {
            this._contextManager = contextManager;
        }

        public IContextManager ContextManager
        {
            get { return this._contextManager; }
        }

        public IContext Context
        {
            get
            {
                IDictionary map = GetMap();
                if (map == null) {
                    return null;
                }
                else {
                    return (IContext)map[_contextManager.Id];
                }
            }
            set
            {
                IDictionary map = GetMap();
                if (map == null) {
                    map = Hashtable.Synchronized(new Hashtable());
                    SetMap(map);
                }
                map[_contextManager.Id] = value;
            }
        }

        protected abstract IDictionary GetMap();

        protected abstract void SetMap(IDictionary value);

        public static void Bind(IContext context)
        {
            GetCurrentContext(context.ContextManager).Context = context;
        }

        public static bool HasBind(IContextManager contextManager)
        {
            return GetCurrentContext(contextManager).Context != null;
        }

        public static IContext Unbind(IContextManager contextManager)
        {
            var removedContext = GetCurrentContext(contextManager).Context;
            GetCurrentContext(contextManager).Context = null;
            return removedContext;
        }

        private static CurrentContext GetCurrentContext(IContextManager contextManager)
        {
            if (contextManager.CurrentContext == null) {
                throw new ArgumentNullException("CurrentContext", "No current context configured.");
            }

            var currentContext = (CurrentContext)contextManager.CurrentContext;
            if (currentContext == null) {
                throw new ArgumentException("Current context does not extend class CurrentContext.");
            }

            return currentContext;
        }

        IContext ICurrentContext.CurrentContext()
        {
            if (this.Context == null) {
                throw new ArgumentNullException("Context", "No object bound to the current context");
            }
            return this.Context;
        }
    }
}

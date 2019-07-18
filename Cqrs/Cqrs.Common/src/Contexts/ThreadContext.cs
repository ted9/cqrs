using System;
using System.Collections;

namespace Cqrs.Contexts
{
    internal class ThreadContext : CurrentContext
    {
        public ThreadContext(IContextManager factory)
            : base(factory)
        { }

        [ThreadStatic]
        private static IDictionary context;

        protected override IDictionary GetMap()
        {
            return context;
        }

        protected override void SetMap(IDictionary value)
        {
            context = value;
        }    
    }
}

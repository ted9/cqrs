using System.Collections;

namespace Cqrs.Contexts
{
    internal class CallContext : CurrentContext
    {
        private const string DatabaseFactoryMapKey = "Cqrs.CallContext";

        public CallContext(IContextManager factory) 
            : base(factory) 
        { }

        protected override IDictionary GetMap()
        {
            return System.Runtime.Remoting.Messaging.CallContext.GetData(DatabaseFactoryMapKey) as IDictionary;
        }

        protected override void SetMap(IDictionary value)
        {
            System.Runtime.Remoting.Messaging.CallContext.SetData(DatabaseFactoryMapKey, value);
        }
    }
}

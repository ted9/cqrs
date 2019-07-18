using System.Collections;
using System.Web;

namespace Cqrs.Contexts
{
    internal class WebContext : CurrentContext
    {
        private const string DatabaseFactoryMapKey = "Cqrs.WebContext";

        public WebContext(IContextManager factory)
            : base(factory)
        { }

        protected override IDictionary GetMap()
        {
            return HttpContext.Current.Items[DatabaseFactoryMapKey] as IDictionary;
        }

        protected override void SetMap(IDictionary value)
        {
            HttpContext.Current.Items[DatabaseFactoryMapKey] = value;
        }
    }
}

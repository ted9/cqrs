using System;
using System.Configuration;

namespace Cqrs.Contexts
{
    public abstract class ContextManager : IContextManager
    {
        protected ContextManager()
            : this(null)
        { }
        protected ContextManager(string contextType)
        {
            this.Id = Guid.NewGuid();
            this.ContextType = ConfigurationManager.AppSettings["thinkcfg.context_type"].Safe("web");
        }

        public Guid Id
        {
            get;
            private set;
        }

        private string _contextType;
        protected internal string ContextType
        {
            get { return this._contextType; }
            set { this._contextType = value; }
        }

        private ICurrentContext _currentContext;
        public ICurrentContext CurrentContext
        {
            get
            {
                if (_currentContext != null)
                    return _currentContext;

                switch (_contextType) {
                    case "web":
                        _currentContext = new WebContext(this);
                        break;
                    case "wcf":
                        _currentContext = new OperationContext(this);
                        break;
                    case "call":
                        _currentContext = new CallContext(this);
                        break;
                    case "thread":
                        _currentContext = new ThreadContext(this);
                        break;
                    default:
                        if (!string.IsNullOrEmpty(_contextType)) {
                            _currentContext = (ICurrentContext)Activator.CreateInstance(Type.GetType(_contextType), this);
                        }
                        break;
                }

                return _currentContext;
            }
        }
    }
}


namespace Cqrs.Contexts
{
    public interface IContext : System.IDisposable
    {
        IContextManager ContextManager { get; }
    }
}

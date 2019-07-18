using System;

namespace Cqrs.Contexts
{
    public interface IContextManager
    {
        Guid Id { get; }
        ICurrentContext CurrentContext { get; }
    }
}

using System;
using Cqrs.Components;

namespace Cqrs.Infrastructure.Logging
{
    [RequiredComponent(typeof(DefaultLoggerFactory))]
    public interface ILoggerFactory
    {
        ILogger GetOrCreate(string name);
        ILogger GetOrCreate(Type type);
    }    
}

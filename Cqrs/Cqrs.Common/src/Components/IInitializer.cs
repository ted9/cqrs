using System;
using System.Collections.Generic;

namespace Cqrs.Components
{
    public interface IInitializer
    {
        void Initialize(IContainer container, IEnumerable<Type> types);
    }
}

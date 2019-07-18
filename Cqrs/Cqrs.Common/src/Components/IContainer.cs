using System;
using System.Collections.Generic;

namespace Cqrs.Components
{
    public interface IContainer : IDisposable
    {
        bool IsRegistered(Type type);
        bool IsRegistered(Type type, string name);

        void RegisterInstance(Type type, object instance);
        void RegisterInstance(Type type, object instance, string name);

        void RegisterType(Type type, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton);
        void RegisterType(Type type, string name, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton);
        void RegisterType(Type from, Type to, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton);
        void RegisterType(Type from, Type to, string name, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton);

        object Resolve(Type type);
        object Resolve(Type type, string name);
        IEnumerable<object> ResolveAll(Type type);
    }
}

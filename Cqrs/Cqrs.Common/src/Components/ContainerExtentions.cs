using System.Collections.Generic;
using System.Linq;

namespace Cqrs.Components
{
    public static class ContainerExtentions
    {
        public static void RegisterInstance<T>(this IContainer that, T instance)
        {
            that.RegisterInstance(typeof(T), instance);
        }

        public static void RegisterInstance<T>(this IContainer that, string name, T instance)
        {
            that.RegisterInstance(typeof(T), instance, name);
        }

        public static void RegisterType<T>(this IContainer that, LifetimeStyle lifetimeStyle)
        {
            that.RegisterType(typeof(T), lifetimeStyle);
        }
        public static void RegisterType<T>(this IContainer that, string name, LifetimeStyle lifetimeStyle)
        {
            that.RegisterType(typeof(T), name, lifetimeStyle);
        }
        public static void RegisterType<TFrom, TTo>(this IContainer that, LifetimeStyle lifetime)
        {
            that.RegisterType(typeof(TFrom), typeof(TTo), lifetime);
        }
        public static void RegisterType<TFrom, TTo>(this IContainer that, string name, LifetimeStyle lifetime)
        {
            that.RegisterType(typeof(TFrom), typeof(TTo), name, lifetime);
        }

        public static bool IsRegistered<T>(this IContainer that)
        {
            return that.IsRegistered(typeof(T));
        }
        public static bool IsRegistered<T>(this IContainer that, string name)
        {
            return that.IsRegistered(typeof(T), name);
        }

        public static T Resolve<T>(this IContainer that)
        {
            return (T)that.Resolve(typeof(T));
        }
        public static T Resolve<T>(this IContainer that, string name)
        {
            return (T)that.Resolve(typeof(T), name);
        }
        public static IEnumerable<T> ResolveAll<T>(this IContainer that)
        {
            return that.ResolveAll(typeof(T)).Cast<T>().ToArray();
        }
    }
}

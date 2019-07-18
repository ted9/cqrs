using System;
using System.Collections.Generic;

namespace Cqrs.Components
{
    public abstract class ObjectContainer : DisposableObject, IContainer
    {
        public static IContainer Instance
        {
            get;
            internal set;
        }

        public virtual object Resolve(Type type)
        {
            return this.Resolve(type, (string)null);
        }
        public abstract object Resolve(Type type, string name);
        public abstract IEnumerable<object> ResolveAll(Type type);

        protected static void CheckInstanceIsAssignableFromType(Type type, object instance)
        {
            if (!type.IsAssignableFrom(instance.GetType()))
                throw new InvalidOperationException(string.Format("请确定类型 {0} 是否可以从指定 {1} 的实例分配。", type.AssemblyQualifiedName, instance.GetType().AssemblyQualifiedName));
        }

        protected static void CheckTypeIsAssignableFromType(Type type, Type sourceType)
        {
            if (!type.IsAssignableFrom(sourceType))
                throw new InvalidOperationException(string.Format("请确定类型 {0} 是否可以从指定 {1} 的实例分配。", type.AssemblyQualifiedName, sourceType.AssemblyQualifiedName));
        }

        public virtual void RegisterInstance(Type type, object instance)
        {
            this.RegisterInstance(type, instance, null);
        }
        public abstract void RegisterInstance(Type type, object instance, string name);

        public virtual void RegisterType(Type type, LifetimeStyle lifetime)
        {
            this.RegisterType(type, (string)null, lifetime);
        }             
        public virtual void RegisterType(Type type, string name, LifetimeStyle lifetime)
        {
            this.RegisterType(type, type, name, lifetime);
        }
        public virtual void RegisterType(Type from, Type to, LifetimeStyle lifetime)
        {
            this.RegisterType(from, to, (string)null, lifetime);
        }
        public abstract void RegisterType(Type from, Type to, string name, LifetimeStyle lifetime);


        public virtual bool IsRegistered(Type type)
        {
            return this.IsRegistered(type, (string)null);
        }
        public abstract bool IsRegistered(Type type, string name);
    }
}

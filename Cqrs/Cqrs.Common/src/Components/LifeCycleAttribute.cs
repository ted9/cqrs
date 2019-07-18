using System;
using System.Reflection;

namespace Cqrs.Components
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class LifeCycleAttribute : Attribute
    {
        public LifeCycleAttribute()
            : this(LifetimeStyle.Singleton)
        { }

        public LifeCycleAttribute(LifetimeStyle lifetime)
        {
            this.Lifetime = lifetime;
        }

        public LifetimeStyle Lifetime { get; private set; }

        public static LifetimeStyle GetLifetimeStyle(Type type)
        {
            if (!type.IsDefined(typeof(LifeCycleAttribute), false))
                return LifetimeStyle.Singleton;

            return type.GetAttribute<LifeCycleAttribute>(false).Lifetime;
        }
    }
}

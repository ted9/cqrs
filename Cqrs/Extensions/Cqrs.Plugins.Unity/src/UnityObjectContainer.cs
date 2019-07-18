using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Cqrs.Components;

namespace Cqrs.Plugins.Unity
{
    public class UnityObjectContainer : ObjectContainer
    {
        internal static LifetimeManager GetLifetime(LifetimeStyle lifestyle)
        {
            switch (lifestyle) {
                case LifetimeStyle.Singleton:
                    return new ContainerControlledLifetimeManager();
                default:
                    return new TransientLifetimeManager();
            }
        }


        private readonly IUnityContainer container;

        public UnityObjectContainer()
            : this(new UnityContainer())
        { }

        public UnityObjectContainer(IUnityContainer container)
        {
            this.container = container;
        }

        public IUnityContainer UnityContainer
        {
            get { return this.container; }
        }


        public override bool IsRegistered(Type type, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) {
                return container.IsRegistered(type);
            }
            return container.IsRegistered(type, name);
        }


        public override void RegisterInstance(Type type, object instance, string name)
        {
            CheckInstanceIsAssignableFromType(type, instance);

            LifetimeManager lifetime = new ContainerControlledLifetimeManager();

            if (string.IsNullOrWhiteSpace(name)) {
                container.RegisterInstance(type, instance, lifetime);
            }
            else {
                container.RegisterInstance(type, name, instance, lifetime);
            }
        }


        public override void RegisterType(Type type, string name, LifetimeStyle lifetime)
        {
            var lifetimeStyle = GetLifetime(lifetime);
            var injectionMembers = InterceptionBehaviorMap.Instance.GetBehaviorTypes(type)
                .Select(behaviorType => new InterceptionBehavior(behaviorType))
                .Cast<InterceptionMember>().ToList();

            if (injectionMembers.Count > 0) {
                if (type.IsSubclassOf(typeof(MarshalByRefObject))) {
                    injectionMembers.Insert(0, new Interceptor<TransparentProxyInterceptor>());
                }
                else {
                    injectionMembers.Insert(0, new Interceptor<VirtualMethodInterceptor>());
                }
            }

            if (type.IsDefined<HandlerAttribute>(false) ||
                type.GetMembers().Any(item => item.IsDefined<HandlerAttribute>(false))) {
                int position = injectionMembers.Count > 0 ? 1 : 0;
                injectionMembers.Insert(position, new InterceptionBehavior<PolicyInjectionBehavior>());
            }

            if (string.IsNullOrWhiteSpace(name)) {
                container.RegisterType(type, lifetimeStyle, injectionMembers.ToArray());
            }
            else {
                container.RegisterType(type, name, lifetimeStyle, injectionMembers.ToArray());
            }
        }

        public override void RegisterType(Type from, Type to, string name, LifetimeStyle lifetime)
        {
            CheckTypeIsAssignableFromType(from, to);

            var lifetimeStyle = GetLifetime(lifetime);
            var serviceBehaviorTypes = InterceptionBehaviorMap.Instance.GetBehaviorTypes(from);
            var implBehaviorTypes = InterceptionBehaviorMap.Instance.GetBehaviorTypes(to);

            var injectionMembers = serviceBehaviorTypes.Union(implBehaviorTypes)
                .Select(behaviorType => new InterceptionBehavior(behaviorType))
                .Cast<InterceptionMember>().ToList();
            if (injectionMembers.Count > 0) {
                if (implBehaviorTypes.Length > 0) {
                    if (to.IsSubclassOf(typeof(MarshalByRefObject))) {
                        injectionMembers.Insert(0, new Interceptor<TransparentProxyInterceptor>());
                    }
                    else {
                        injectionMembers.Insert(0, new Interceptor<VirtualMethodInterceptor>());
                    }
                }
                if (serviceBehaviorTypes.Length > 0 && from.IsInterface) {
                    injectionMembers.Insert(0, new Interceptor<InterfaceInterceptor>());
                }
            }

            if (to.IsDefined<HandlerAttribute>(false) ||
                to.GetMembers().Any(item => item.IsDefined<HandlerAttribute>(false))) {
                int position = injectionMembers.Count > 0 ? 1 : 0;
                injectionMembers.Insert(position, new InterceptionBehavior<PolicyInjectionBehavior>());
            }

            if (string.IsNullOrWhiteSpace(name)) {
                container.RegisterType(from, to, lifetimeStyle, injectionMembers.ToArray());
            }
            else {
                container.RegisterType(from, to, name, lifetimeStyle, injectionMembers.ToArray());
            }
        }

        public override object Resolve(Type type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return container.Resolve(type);

            return container.Resolve(type, name);
        }

        public override IEnumerable<object> ResolveAll(Type type)
        {
            return container.ResolveAll(type);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                container.Dispose();
        }
    }
}

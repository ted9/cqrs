using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Cqrs.Components;


namespace Cqrs.Messaging.Handling
{
    /// <summary>
    /// <see cref="IMessageHandlerProvider"/> 的默认实现
    /// </summary>
    public class DefaultHandlerProvider : IHandlerProvider, IInitializer
    {
        private readonly Type _handlerGenericType;
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public DefaultHandlerProvider()
        {
            this._handlerGenericType = typeof(IHandler<>);
        }

        void IInitializer.Initialize(IContainer container, IEnumerable<Type> types)
        {
            //var types = assemblies.SelectMany(assembly => assembly.GetTypes());
            types.Where(IsRegisterType).ForEach(type => RegisterType(type, container));
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        private void RegisterType(Type type, IContainer container)
        {
            var interfaceTypes = type.GetInterfaces().Where(IsGenericType);
            foreach (var interfaceType in interfaceTypes) {
                LifetimeStyle lifetime = LifetimeStyle.Transient;

                if (type.IsDefined(typeof(LifeCycleAttribute), false)) {
                    lifetime = type.GetAttribute<LifeCycleAttribute>(false).Lifetime;
                }

                container.RegisterType(interfaceType, type, type.FullName, lifetime);
            }
        }
        /// <summary>
        /// 判断是否为事件处理接口
        /// </summary>
        private bool IsGenericType(Type type)
        {
            return type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == _handlerGenericType;
        }

        /// <summary>
        /// 判断是否为事件处理程序类型
        /// </summary>
        private bool IsRegisterType(Type type)
        {
            return type.IsClass && !type.IsAbstract &&
                type.GetInterfaces().Any(IsGenericType);
        }

        /// <summary>
        /// 获取该事件类型的处理器
        /// </summary>
        public IEnumerable<IProxyHandler> GetHandlers(Type type)
        {
            var handlerType = _handlerGenericType.MakeGenericType(type);

            return ObjectContainer.Instance.ResolveAll(handlerType)
                .Select(handler => {
                    var handlerWrapperType = typeof(HandlerWrapper<>).MakeGenericType(type);
                    return Activator.CreateInstance(handlerWrapperType, new[] { handler });
                }).OfType<IProxyHandler>().ToArray();
        }
    }
}

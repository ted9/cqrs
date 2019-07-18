using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Web;
using Cqrs.Components;
using TinyIoC;

namespace CqrsSample.Infrastructure
{
    #region
    
    internal class TinyContainer : ObjectContainer
    {
        TinyIoCContainer container = new TinyIoCContainer();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                container.Dispose();
        }

        #region IObjectContainer 成员

        public override bool IsRegistered(Type type, string name)
        {
            return container.CanResolve(type, name, ResolveOptions.FailNameNotFoundOnly);
        }


        public override void RegisterInstance(Type type, object instance, string name)
        {
            CheckInstanceIsAssignableFromType(type, instance);

            container.Register(type, instance, name);
        }
        //public IContainer RegisterInstance(Type type, object instance)
        //{
        //    container.Register(type, instance);

        //    return this;
        //}

        //public IContainer RegisterInstance(Type type, object instance, string name)
        //{
        //    container.Register(type, instance, name);

        //    return this;
        //}

        //public IContainer RegisterType(Type type, LifetimeStyle lifetimeStyle)
        //{
        //    return this.RegisterType(type, (string)null, lifetimeStyle);
        //}

        //public IContainer RegisterType(Type type, string name, LifetimeStyle lifetimeStyle)
        //{
        //    switch (lifetimeStyle) {
        //        case LifetimeStyle.Singleton:
        //            container.Register(type, name).AsSingleton();
        //            break;
        //        case LifetimeStyle.Transient:
        //            container.Register(type, name).AsMultiInstance();
        //            break;
        //        case LifetimeStyle.PerRequest:
        //            container.Register(type, name).AsPerSessionSingleton();
        //            break;
        //        case LifetimeStyle.PerThread:
        //            container.Register(type, name).AsPerThreadSingleton();
        //            break;
        //    }

        //    return this;
        //}

        public override void RegisterType(Type type, string name, LifetimeStyle lifetime)
        {
            switch (lifetime) {
                case LifetimeStyle.Singleton:
                    container.Register(type, name).AsSingleton();
                    break;
                case LifetimeStyle.Transient:
                    container.Register(type, name).AsMultiInstance();
                    break;
            }
        }

        //public IContainer RegisterType(Type from, Type to, LifetimeStyle lifetimeStyle)
        //{
        //    return this.RegisterType(from, to, (string)null, lifetimeStyle);
        //}
        //public IContainer RegisterType(Type from, Type to, string name, LifetimeStyle lifetimeStyle)
        //{
        //    switch (lifetimeStyle) {
        //        case LifetimeStyle.Singleton:
        //            container.Register(from, to, name).AsSingleton();
        //            break;
        //        case LifetimeStyle.Transient:
        //            container.Register(from, to, name).AsMultiInstance();
        //            break;
        //        case LifetimeStyle.PerRequest:
        //            container.Register(from, to, name).AsPerSessionSingleton();
        //            break;
        //        case LifetimeStyle.PerThread:
        //            container.Register(from, to, name).AsPerThreadSingleton();
        //            break;
        //    }

        //    return this;
        //}

        public override void RegisterType(Type from, Type to, string name, LifetimeStyle lifetime)
        {
            CheckTypeIsAssignableFromType(from, to);

            switch (lifetime) {
                case LifetimeStyle.Singleton:
                    container.Register(from, to, name).AsSingleton();
                    break;
                case LifetimeStyle.Transient:
                    container.Register(from, to, name).AsMultiInstance();
                    break;
            }
        }

        public override object Resolve(Type type, string key)
        {
            return container.Resolve(type, key);
        }
        public override IEnumerable<object> ResolveAll(Type type)
        {
            return container.ResolveAll(type, true);
        }
        #endregion
    }
    #endregion
}

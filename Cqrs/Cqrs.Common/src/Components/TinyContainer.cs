using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.Text;
using System.Web;
using TinyIoC;

namespace ThinkNet.Components
{
    #region
    class PerSessionLifetimeProvider : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        class ContainerExtension : IExtension<OperationContext>
        {
            #region Members

            public object Value { get; set; }

            #endregion

            #region IExtension<OperationContext> Members

            public void Attach(OperationContext owner)
            { }

            public void Detach(OperationContext owner)
            { }

            #endregion
        }


        private readonly string key = string.Format("TinyIoC.CurrentContext.{0}", Guid.NewGuid());

        #region ITinyIoCObjectLifetimeProvider 成员

        public object GetObject()
        {
            object result = null;

            //Get object depending on  execution environment ( WCF without HttpContext,HttpContext or CallContext)

            if (OperationContext.Current != null) {
                //WCF without HttpContext environment
                ContainerExtension containerExtension = OperationContext.Current.Extensions.Find<ContainerExtension>();
                if (containerExtension != null) {
                    result = containerExtension.Value;
                }
            }
            else if (HttpContext.Current != null) {
                //HttpContext avaiable ( ASP.NET ..)
                if (HttpContext.Current.Items[key] != null)
                    result = HttpContext.Current.Items[key];
            }
            else {
                //Not in WCF or ASP.NET Environment, UnitTesting, WinForms, WPF etc.
                result = CallContext.GetData(key);
            }

            return result;
            //return CallContext.GetData(key);
        }

        public void SetObject(object newValue)
        {
            if (OperationContext.Current != null) {
                //WCF without HttpContext environment
                ContainerExtension containerExtension = OperationContext.Current.Extensions.Find<ContainerExtension>();
                if (containerExtension == null) {
                    containerExtension = new ContainerExtension() {
                        Value = newValue
                    };

                    OperationContext.Current.Extensions.Add(containerExtension);
                }
            }
            else if (HttpContext.Current != null) {
                //HttpContext avaiable ( ASP.NET ..)
                if (HttpContext.Current.Items[key] == null)
                    HttpContext.Current.Items[key] = newValue;
            }
            else {
                //Not in WCF or ASP.NET Environment, UnitTesting, WinForms, WPF etc.
                CallContext.SetData(key, newValue);
            }          
        }

        public void ReleaseObject()
        {
            if (OperationContext.Current != null) {
                //WCF without HttpContext environment
                ContainerExtension containerExtension = OperationContext.Current.Extensions.Find<ContainerExtension>();
                if (containerExtension != null) {
                    object obj = containerExtension.Value;
                    if (obj is IDisposable) {
                        (obj as IDisposable).Dispose();
                    }
                    OperationContext.Current.Extensions.Remove(containerExtension);
                }

            }
            else if (HttpContext.Current != null) {
                //HttpContext avaiable ( ASP.NET ..)
                if (HttpContext.Current.Items[key] != null) {
                    if (HttpContext.Current.Items[key] is IDisposable) {
                        ((IDisposable)HttpContext.Current.Items[key]).Dispose();
                    }
                    HttpContext.Current.Items[key] = null;
                }
            }
            else {
                //Not in WCF or ASP.NET Environment, UnitTesting, WinForms, WPF etc.
                CallContext.FreeNamedDataSlot(key);
            }
        }

        #endregion
    }

    class PerThreadLifetimeProvider : TinyIoCContainer.ITinyIoCObjectLifetimeProvider
    {
        [ThreadStatic]
        private static Dictionary<Guid, object> values;
        private readonly Guid key;

        public PerThreadLifetimeProvider()
        {
            this.key = Guid.NewGuid();
        }

        #region ITinyIoCObjectLifetimeProvider 成员

        public object GetObject()
        {
            EnsureValues();

            object result;
            values.TryGetValue(this.key, out result);
            return result;

        }

        public void SetObject(object value)
        {
            EnsureValues();

            values[this.key] = value;
        }

        public void ReleaseObject()
        {
            if (values != null) {
                values.Values.OfType<IDisposable>().ForEach(obj => obj.Dispose());
                values.Clear();
            }
        }

        #endregion

        private static void EnsureValues()
        {
            // no need for locking, values is TLS
            if (values == null) {
                values = new Dictionary<Guid, object>();
            }
        }

    }

    static class TinyIoCAspNetExtensions
    {
        public static TinyIoCContainer.RegisterOptions AsPerSessionSingleton(this TinyIoCContainer.RegisterOptions registerOptions)
        {
            return TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registerOptions, new PerSessionLifetimeProvider(), "per session singleton");
        }

        public static TinyIoCContainer.MultiRegisterOptions AsPerSessionMultiInstance(this TinyIoCContainer.MultiRegisterOptions registerOptions)
        {
            return TinyIoCContainer.MultiRegisterOptions.ToCustomLifetimeManager(registerOptions, new PerSessionLifetimeProvider(), "per session singleton");
        }

        public static TinyIoCContainer.RegisterOptions AsPerThreadSingleton(this TinyIoCContainer.RegisterOptions registerOptions)
        {
            return TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(registerOptions, new PerThreadLifetimeProvider(), "per thread singleton");
        }

        public static TinyIoCContainer.MultiRegisterOptions AsPerThreadMultiInstance(this TinyIoCContainer.MultiRegisterOptions registerOptions)
        {
            return TinyIoCContainer.MultiRegisterOptions.ToCustomLifetimeManager(registerOptions, new PerThreadLifetimeProvider(), "per thread singleton");
        }
    }

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
                case LifetimeStyle.PerRequest:
                    container.Register(type, name).AsPerSessionSingleton();
                    break;
                case LifetimeStyle.PerThread:
                    container.Register(type, name).AsPerThreadSingleton();
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
                case LifetimeStyle.PerRequest:
                    container.Register(from, to, name).AsPerSessionSingleton();
                    break;
                case LifetimeStyle.PerThread:
                    container.Register(from, to, name).AsPerThreadSingleton();
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

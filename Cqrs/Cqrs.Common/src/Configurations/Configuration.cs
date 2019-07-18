using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cqrs.Components;
using Cqrs.Infrastructure.Utilities;


namespace Cqrs.Configurations
{
    public sealed class Configuration
    {
        //public static Configuration Create()
        //{
        //    return new Configuration(new TinyContainer());
        //}
        public static Configuration Create(Func<IContainer> provider)
        {
            return new Configuration(provider.Invoke());
        }


        private readonly List<Assembly> _assemblies;
        private readonly List<IInitializer> _initializers;
        private readonly List<Type> _initializeTypes;
        private readonly HashSet<TypeRegistration> _registeredTypes;
        private readonly IContainer _container;
        private Configuration(IContainer container)
        {
            this._container = ObjectContainer.Instance = container;
            this._assemblies = new List<Assembly>();
            this._initializers = new List<IInitializer>();
            this._initializeTypes = new List<Type>();
            this._registeredTypes = new HashSet<TypeRegistration>();
        }


        ///// <summary>
        ///// 对象容器
        ///// </summary>
        //public IObjectContainer Container { get; private set; }


        /// <summary>
        /// 加载程序集
        /// </summary>
        public Configuration LoadAssemblies(params Assembly[] assemblies)
        {
            _assemblies.Clear();
            _assemblies.AddRange(assemblies);

            return this;
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        public Configuration LoadAssemblies(params string[] assemblies)
        {
            return this.LoadAssemblies(Array.ConvertAll(assemblies, Assembly.Load));
        }

        /// <summary>
        /// 扫描bin目录的程序集
        /// </summary>
        public Configuration LoadAssemblies()
        {
            string applicationAssemblyDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin");
            if (!FileUtils.DirectoryExists(applicationAssemblyDirectory)) {
                applicationAssemblyDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }


            var assemblies = Directory.GetFiles(applicationAssemblyDirectory)
                .Where(file => {
                    var ext = Path.GetExtension(file).ToLower();
                    return ext.EndsWith(".dll") || ext.EndsWith(".exe");
                })
                .Select(Assembly.LoadFrom)
                //.Where(assembly => assembly.IsDefined<ParticipateInRuntimeAttribute>(false))
                //.OrderBy(assembly => assembly.GetAttribute<ParticipateInRuntimeAttribute>(false).Order)
                .ToArray();

            return this.LoadAssemblies(assemblies);
        }

        private bool _running = false;
        /// <summary>
        /// 配置完成。
        /// </summary>
        public void Done()
        {
            if (_running)
                return;

            if (_assemblies.Count == 0) {
                this.LoadAssemblies();
            }


            var allTypes = _assemblies.SelectMany(assembly => assembly.GetTypes()).ToArray();

            allTypes.Where(IsRegisteredComponent).ForEach(RegisterComponent);
            allTypes.Where(IsRequiredComponent).ForEach(RegisterRequiredComponent);

            _initializeTypes.Select(_container.Resolve).OfType<IInitializer>().Concat(_initializers)
                .ForEach(initializer => {
                    initializer.Initialize(_container, allTypes);
                });
            
            _assemblies.Clear();
            _initializers.Clear();
            _initializeTypes.Clear();
            _registeredTypes.Clear();


            _running = true;
        }

        /// <summary>
        /// 注册实例
        /// </summary>
        public Configuration RegisterInstance<T>(T instance, string name = null)
        {
            return this.RegisterInstance(typeof(T), instance, name);
        }
        /// <summary>
        /// 注册实例
        /// </summary>
        public Configuration RegisterInstance(Type serviceType, object instance, string name = null)
        {
            if (_running) {
                throw new InvalidOperationException("系统已经启动，不能执行类型注册，请在初始化的时候操作。");
            }

            if (!_registeredTypes.Contains(new TypeRegistration(serviceType, name)) || !_container.IsRegistered(serviceType, name)) {
                if (string.IsNullOrWhiteSpace(name)) {
                    _container.RegisterInstance(serviceType, instance);
                }
                else {
                    _container.RegisterInstance(serviceType, instance, name);
                }

                _registeredTypes.Add(new TypeRegistration(serviceType, name));

                if (IsInitializer(instance)) {
                    _initializers.Add((IInitializer)instance);
                }
            }

            return this;
        }
        /// <summary>
        /// 注册类型的实例
        /// </summary>
        public Configuration RegisterInstance(object instance, params Type[] types)
        {
            types.ForEach(type => {
                RegisterInstance(type, instance);
            });

            return this;
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public Configuration RegisterType<T>(LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton, string name = null)
        {
            return this.RegisterInstance(typeof(T), lifetimeStyle, name);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public Configuration RegisterType(Type serviceType, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton, string name = null)
        {
            return this.RegisterType(serviceType, serviceType, lifetimeStyle, name);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public Configuration RegisterType<TFrom, TTo>(LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton, string name = null)
        {
            return this.RegisterType(typeof(TFrom), typeof(TTo), lifetimeStyle, name);
        }
        /// <summary>
        /// 注册类型
        /// </summary>
        public Configuration RegisterType(Type from, Type to, LifetimeStyle lifetimeStyle = LifetimeStyle.Singleton, string name = null)
        {
            if (_running) {
                throw new InvalidOperationException("系统已经启动，不能执行类型注册，请在初始化的时候操作。");
            }
            
            if (!(_registeredTypes.Contains(new TypeRegistration(from, name)) || _container.IsRegistered(from, name))) {
                if (string.IsNullOrWhiteSpace(name)) {
                    _container.RegisterType(from, to, lifetimeStyle);
                }
                else {
                    _container.RegisterType(from, to, name, lifetimeStyle);
                }

                _registeredTypes.Add(new TypeRegistration(from, name));

                if (lifetimeStyle == LifetimeStyle.Singleton && IsInitializerType(to)) {
                    _initializeTypes.Add(from);
                }
            }

            return this;
        }


        private static bool IsInitializerType(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IInitializer).IsAssignableFrom(type);
        }

        private static bool IsInitializer(object instance)
        {
            return instance is IInitializer;
        }

        private bool IsRegisteredComponent(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsDefined<RegisteredComponentAttribute>(false);
        }

        private bool IsRequiredComponent(Type type)
        {
            return type.IsDefined<RequiredComponentAttribute>(false);
        }

        private void RegisterComponent(Type type)
        {
            var components = type.GetAttributes<RegisteredComponentAttribute>(false);
            foreach (var component in components) {
                var name = component.GetFinalRegisterName(type);
                var lifetimeStyle = LifeCycleAttribute.GetLifetimeStyle(type);
                var serviceType = component.ServiceType;

                if (serviceType == null) {
                    this.RegisterType(type, lifetimeStyle, name);
                }
                else {
                    this.RegisterType(serviceType, type, lifetimeStyle, name);
                }
            }
        }

        private void RegisterRequiredComponent(Type type)
        {
            var components = type.GetAttributes<RequiredComponentAttribute>(false);
            foreach (var component in components) {
                var name = component.GetFinalRegisterName();
                var lifetimeStyle = LifeCycleAttribute.GetLifetimeStyle(type);
                var serviceType = component.ServiceType;

                if (lifetimeStyle == LifetimeStyle.Singleton) {
                    var bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;
                    var filed = serviceType.GetField("Instance", bindingFlags);
                    if (filed != null) {
                        this.RegisterInstance(type, filed.GetValue(null), name);
                        continue;
                    }
                    var property = serviceType.GetProperty("Instance", bindingFlags);
                    if (property != null) {
                        this.RegisterInstance(type, property.GetValue(null, null), name);
                        continue;
                    }

                    if (component.CreateInstance) {
                        var instance = component.ConstructorParameters == null || component.ConstructorParameters.Length == 0 ?
                            Activator.CreateInstance(serviceType) : Activator.CreateInstance(serviceType, component.ConstructorParameters);
                        this.RegisterInstance(type, instance, name);
                        continue;
                    }
                }

                if (serviceType == null) {
                    this.RegisterType(type, lifetimeStyle, name);
                }
                else {
                    this.RegisterType(type, serviceType, lifetimeStyle, name);
                }
            }
        }


        class TypeRegistration
        {
            private int _hashCode;
            private Type _type;
            private string _name;

            public Type Type { get { return _type; } }
            public string Name { get { return _name; } }

            public TypeRegistration(Type type)
                : this(type, null)
            { }

            public TypeRegistration(Type type, string name)
            {
                this._type = type;
                this._name = name;

                this._hashCode = String.Concat(type.FullName, "|", name).GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var typeRegistration = obj as TypeRegistration;

                if (typeRegistration == null)
                    return false;

                if (Type != typeRegistration.Type)
                    return false;

                if (String.Compare(Name, typeRegistration.Name, StringComparison.Ordinal) != 0)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}

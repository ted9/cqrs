using System;

namespace Cqrs.Components
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredComponentAttribute : Attribute
    {
        public RequiredComponentAttribute(Type serviceType)
        {
            Ensure.NotNull(serviceType, "serviceType");

            _serviceType = serviceType;
        }

        public RequiredComponentAttribute(Type serviceType, string registerName)
            : this(serviceType)
        {
            Ensure.NotNullOrWhiteSpace(registerName, "registerName");

            _registerName = registerName;
        }


        private string _registerName;
        public string RegisterName
        {
            get { return _registerName; }
        }

        private Type _serviceType;
        public Type ServiceType
        {
            get { return _serviceType; }
        }

        private bool _registerTypeName = false;
        public bool RegisterTypeName
        {
            get { return _registerTypeName; }
            set { _registerTypeName = value; }
        }

        private bool _created = false;
        public bool CreateInstance
        {
            get { return _created; }
            set { _created = value; }
        }

        private object[] _parameters = new object[0];
        public object[] ConstructorParameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public string GetFinalRegisterName()
        {
            return _registerTypeName ? _serviceType.FullName : _registerName;
        }
    }
}

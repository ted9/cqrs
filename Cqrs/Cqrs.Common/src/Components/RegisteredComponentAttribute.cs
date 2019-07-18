using System;


namespace Cqrs.Components
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisteredComponentAttribute : Attribute
    {
        public RegisteredComponentAttribute()
        { }
        public RegisteredComponentAttribute(string registerName)
        {
            Ensure.NotNullOrWhiteSpace(registerName, "registerName");

            _registerName = registerName;
        }
        public RegisteredComponentAttribute(Type serviceType)
        {
            Ensure.NotNull(serviceType, "serviceType");

            _serviceType = serviceType;
        }
        public RegisteredComponentAttribute(Type serviceType, string registerName)
            : this(serviceType)
        {
            Ensure.NotNullOrWhiteSpace(registerName, "registerName");

            _registerName = registerName;
        }

        private Type _serviceType;
        public Type ServiceType
        {
            get { return _serviceType; }
        }

        private string _registerName;
        public string RegisterName
        {
            get { return _registerName; }
        }

        private bool _registerTypeName = false;
        public bool RegisterTypeName
        {
            get { return _registerTypeName; }
            set { _registerTypeName = value; }
        }

        public string GetFinalRegisterName(Type registerType)
        {
            return _registerTypeName ? registerType.FullName : _registerName;
        }


            
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

using Cqrs.Components;


namespace Cqrs.Infrastructure
{
    public abstract class AbstractTypeCodeProvider : ITypeCodeProvider, IInitializer
    {
        private readonly IDictionary<int, Type> _codeTypeDict;
        private readonly IDictionary<Type, int> _typeCodeDict;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected AbstractTypeCodeProvider()
        {
            this._codeTypeDict = new Dictionary<int, Type>();
            this._typeCodeDict = new Dictionary<Type, int>();
        }

        /// <summary>
        /// Get the code of the given type.
        /// </summary>
        public int GetTypeCode(Type type)
        {
            return _typeCodeDict[type];
        }
        /// <summary>
        /// Get the type of the given type code.
        /// </summary>
        public Type GetType(int typeCode)
        {
            return _codeTypeDict[typeCode];
        }

        protected void RegisterType<T>(int code)
        {
            RegisterType(code, typeof(T));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="type"></param>
        protected void RegisterType(int code, Type type)
        {
            if (_codeTypeDict.ContainsKey(code))
                return;

            _codeTypeDict.Add(code, type);
            _typeCodeDict.Add(type, code);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected abstract bool MatchedType(Type type);


        private void RegisterType(Type type)
        {
            if (_typeCodeDict.ContainsKey(type))
                return;

            int code = type.FullName.GetHashCode();

            _codeTypeDict.Add(code, type);
            _typeCodeDict.Add(type, code);
        }

        void IInitializer.Initialize(IContainer container, IEnumerable<Type> types)
        {
            //var types = assemblies.SelectMany(assembly => assembly.GetTypes());
            types.Where(MatchedType).ForEach(RegisterType);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using System.Web.Script.Serialization;

using Cqrs.Components;
using Cqrs.Infrastructure.Utilities;


namespace Cqrs.Infrastructure.Serialization
{
    internal class DefaultTextSerializer : ITextSerializer, IInitializer
    {
        private readonly JavaScriptSerializer _jsonSerializer;

        public DefaultTextSerializer()
        {
            this._jsonSerializer = new JavaScriptSerializer(new CustomTypeResolver());
        }

        public void Serialize(TextWriter writer, object obj)
        {
            writer.Write(_jsonSerializer.Serialize(obj));
            writer.Flush();
        }

        public object Deserialize(TextReader reader)
        {
            return _jsonSerializer.DeserializeObject(reader.ReadToEnd());
        }

        public void Initialize(IContainer container, IEnumerable<Type> types)
        {
            var converterTypes = types.Where(IsConverter);

            if (!converterTypes.Any()) {
                _jsonSerializer.RegisterConverters(new JavaScriptConverter[] { new DefaultJavaScriptConverter(types) });
                return;
            }

            var converters = converterTypes.Select(type => (JavaScriptConverter)(IsInjection(type) ?
                Activator.CreateInstance(type, types) : Activator.CreateInstance(type)));
            _jsonSerializer.RegisterConverters(converters);
        }

        private static bool IsConverter(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(JavaScriptConverter));
        }

        private static bool IsInjection(Type type)
        {
            return type.GetConstructors().Any(constructor => {
                var parameters = constructor.GetParameters();
                return parameters.Count() == 1 && parameters.First().ParameterType == typeof(IEnumerable<Type>);
            });
        }

        class DefaultJavaScriptConverter : JavaScriptConverter
        {
            private readonly IEnumerable<Type> _supportedTypes;
            public DefaultJavaScriptConverter(IEnumerable<Type> types)
            {
                _supportedTypes = types.Where(IsSupportedType).ToArray();
            }

            private static bool IsSupportedType(Type type)
            {
                return type.IsClass && !type.IsAbstract && type.IsDefined<SerializableAttribute>(false);
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get { return _supportedTypes; }
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                return ObjectUtils.GetProperties(obj.GetType())
                    .Where(prop => !prop.IsDefined<ScriptIgnoreAttribute>(false) || prop.CanWrite)
                    .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));
            }
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                var obj = FormatterServices.GetUninitializedObject(type);
                ObjectUtils.GetProperties(type)
                    .ForEach(prop => {
                        if (!prop.CanWrite)
                            return;

                        object value;
                        if (dictionary.TryGetValue(prop.Name, out value) && value != null) {
                            if (prop.PropertyType != value.GetType()) {
                                value = TypeConvert.To(value, prop.PropertyType);
                            }
                            prop.SetValue(obj, value, null);
                        }
                    });

                return obj;
            }
        }

        class CustomTypeResolver : JavaScriptTypeResolver
        {
            public override Type ResolveType(string id)
            {
                return Type.GetType(id, false, true);
            }

            public override string ResolveTypeId(Type type)
            {
                return string.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
            }
        }        
    }
}

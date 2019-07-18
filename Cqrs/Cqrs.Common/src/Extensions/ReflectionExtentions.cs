using System.Linq;

namespace System.Reflection
{
    public static class ReflectionExtentions
    {
        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).FieldType;
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).PropertyType;
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).ReturnType;
            if (memberInfo is ConstructorInfo)
                return null;
            if (memberInfo is Type)
                return (Type)memberInfo;
            throw new ArgumentException();
        }

        public static bool IsStaticMember(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).IsStatic;
            if (memberInfo is PropertyInfo) {
                MethodInfo propertyMethod;
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                if ((propertyMethod = propertyInfo.GetGetMethod()) != null || (propertyMethod = propertyInfo.GetSetMethod()) != null)
                    return IsStaticMember(propertyMethod);

            }
            if (memberInfo is MethodInfo)
                return ((MethodInfo)memberInfo).IsStatic;
            throw new ArgumentException();
        }

        public static object GetMemberValue(this MemberInfo memberInfo, object o)
        {
            if (memberInfo is FieldInfo)
                return ((FieldInfo)memberInfo).GetValue(o);
            if (memberInfo is PropertyInfo)
                return ((PropertyInfo)memberInfo).GetGetMethod().Invoke(o, new object[0]);
            throw new ArgumentException();
        }

        public static void SetMemberValue(this MemberInfo memberInfo, object o, object value)
        {
            if (memberInfo is FieldInfo)
                ((FieldInfo)memberInfo).SetValue(o, value);
            else if (memberInfo is PropertyInfo)
                ((PropertyInfo)memberInfo).GetSetMethod().Invoke(o, new[] { value });
            else throw new ArgumentException();
        }

        public static PropertyInfo GetExposingProperty(this MemberInfo memberInfo)
        {
            var reflectedType = memberInfo.ReflectedType;
            foreach (var propertyInfo in reflectedType.GetProperties()) {
                if (propertyInfo.GetGetMethod() == memberInfo || propertyInfo.GetSetMethod() == memberInfo)
                    return propertyInfo;
            }
            return null;
        }

        public static Type GetFirstInnerReturnType(this MemberInfo memberInfo)
        {
            var type = memberInfo.GetMemberType();

            if (type == null)
                return null;

            if (type.IsGenericType) {
                return type.GetGenericArguments()[0];
            }

            return type;
        }


        public static A[] GetAttributes<A>(this ICustomAttributeProvider provider, bool inherit)
            where A : Attribute
        {
            return provider
                .GetCustomAttributes(typeof(A), inherit)
                .Cast<A>()
                .ToArray();
        }


        public static A GetAttribute<A>(this ICustomAttributeProvider provider, bool inherit)
            where A : Attribute
        {
            var attributes = provider.GetAttributes<A>(inherit);

            if (attributes != null && attributes.Length > 0)
                return (A)attributes[0];
            return null;
        }

        public static bool IsDefined<TAttribute>(this ICustomAttributeProvider provider, bool inherit)
            where TAttribute : Attribute
        {
            return provider.IsDefined(typeof(TAttribute), inherit);
        }
    }
}

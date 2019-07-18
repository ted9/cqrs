using System.Linq;
using System.Reflection;

using Cqrs.Infrastructure.Utilities;

namespace System
{
    public static class TypeExtensions
    {
        public static bool CanBeNull(this Type t)
        {
            return IsNullable(t) || !t.IsValueType;
        }

        public static MemberInfo GetSingleMember(this Type t, string name)
        {
            return GetSingleMember(t, name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static MemberInfo GetSingleMember(this Type t, string name, BindingFlags bindingFlags)
        {
            var members = t.GetMember(name, bindingFlags);
            if (members.Length > 0)
                return members[0];
            return null;
        }

        public static bool IsNullable(this Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNullableType(this Type t)
        {
            return Nullable.GetUnderlyingType(t);
        }

        public static object GetDefault(this Type t)
        {
            return TypeConvert.GetDefault(t);
        }

        public static string GetShortName(this Type t)
        {
            var name = t.Name;
            if (t.IsGenericTypeDefinition)
                return name.Split('`')[0];
            return name;
        }
    }
}

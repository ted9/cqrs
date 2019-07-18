using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Cqrs.Infrastructure.Utilities;


namespace System.Collections.Generic
{
    public static class EnumerableExtentions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source.IsEmpty())
                return;

            foreach (var item in source) {
                action(item);
            }
        }
        public static IEnumerable<T> Safe<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true;
            var collection = source as ICollection;
            if (collection != null)
                return collection.Count == 0;
            return !source.Any();
        }
    }
}

namespace System.Collections
{
    public static class EnumerableExtentions
    {
        internal static readonly IDictionary EmptyDict = new Hashtable();

        public static T ToEntity<T>(this IDictionary dict)
            where T : class
        {
            return dict.ToEntity<T>(EmptyDict);
        }
        public static T ToEntity<T>(this IDictionary dict, IDictionary map)
            where T : class
        {
            return (T)dict.ToEntity(typeof(T), map);
        }
        public static object ToEntity(this IDictionary dict, Type type)
        {
            return dict.ToEntity(type, EmptyDict);
        }
        public static object ToEntity(this IDictionary dict, Type type, IDictionary map)
        {
            if (dict == null || dict.Count == 0)
                return null;

            var entity = FormatterServices.GetUninitializedObject(type);

            ObjectUtils.GetProperties(type)
                .ForEach(property => {
                    string name = map.Contains(property.Name) ? (string)map[property.Name] : property.Name;

                    if (dict[name] != DBNull.Value && dict[name] != null) {
                        property.SetValue(entity, TypeConvert.To(dict[name], property.PropertyType), null);
                    }
                });

            return entity;
        }
        public static List<T> ToEntities<T>(this IList list) where T : class
        {
            List<T> result = new List<T>();

            if (list != null && list.Count != 0) {
                for (int i = 0; i < list.Count; i++) {
                    result.Add((list[i] as IDictionary).ToEntity<T>());
                }
            }

            return result;
        }
        public static List<T> ToEntities<T>(this IList list, IDictionary map) where T : class
        {
            List<T> result = new List<T>();

            if (list != null && list.Count != 0) {
                for (int i = 0; i < list.Count; i++) {
                    result.Add((list[i] as IDictionary).ToEntity<T>(map));
                }
            }

            return result;
        }

    }
}

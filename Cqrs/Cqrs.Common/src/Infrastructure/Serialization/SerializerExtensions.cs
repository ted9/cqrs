using System.IO;

namespace Cqrs.Infrastructure.Serialization
{
    public static class SerializerExtensions
    {
        public static T Deserialize<T>(this IBinarySerializer serializer,  byte[] data) where T : class
        {
            return serializer.Deserialize(data) as T;
        }

        public static string Serialize<T>(this ITextSerializer serializer, T data)
        {
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }

        public static string Serialize(this ITextSerializer serializer, object data)
        {
            using (var writer = new StringWriter()) {
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }

        public static T Deserialize<T>(this ITextSerializer serializer, string serialized)
        {
            using (var reader = new StringReader(serialized)) {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static object Deserialize(this ITextSerializer serializer, string serialized)
        {
            using (var reader = new StringReader(serialized)) {
                return serializer.Deserialize(reader);
            }
        }
    }
}

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cqrs.Infrastructure.Serialization
{
    internal class DefaultBinarySerializer : IBinarySerializer
    {
        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream()) {
                _binaryFormatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data)) {
                return _binaryFormatter.Deserialize(stream);
            }
        }
    }
}

using Cqrs.Components;


namespace Cqrs.Infrastructure.Serialization
{
    [RequiredComponent(typeof(DefaultBinarySerializer))]
    public interface IBinarySerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] data);
    }    
}

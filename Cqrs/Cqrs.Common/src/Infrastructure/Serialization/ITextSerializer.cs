using System.IO;
using Cqrs.Components;

namespace Cqrs.Infrastructure.Serialization
{
    [RequiredComponent(typeof(DefaultTextSerializer))]
    public interface ITextSerializer
    {
        void Serialize(TextWriter writer, object obj);

        object Deserialize(TextReader reader);
    }

    
}

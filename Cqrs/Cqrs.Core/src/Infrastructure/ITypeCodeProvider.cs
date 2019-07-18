using System;
using Cqrs.Components;

namespace Cqrs.Infrastructure
{
    /// <summary>Represents a provider to provide the type and code mapping information.
    /// </summary>
    [RequiredComponent(typeof(DefaultTypeCodeProvider))]
    public interface ITypeCodeProvider
    {
        /// <summary>Get the code of the given type.
        /// </summary>
        int GetTypeCode(Type type);
        /// <summary>Get the type of the given type code.
        /// </summary>
        Type GetType(int typeCode);
    }    
}

using System;

namespace ThinkNet.Components
{
    /// <summary>
    /// 表示该特性的类不会被注册。
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class NonRegisteredAttribute : Attribute
    { }
}

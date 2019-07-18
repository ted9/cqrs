using System;

namespace Cqrs.Caching
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class CachingAttribute : CacheRegionAttribute
    {
        public CachingAttribute(CachingMethod method)
            : this(method, new string[0])
        { }
        public CachingAttribute(CachingMethod method, params string[] relatedAreas)
        {
            this.Method = method;
            this.RelatedAreas = relatedAreas;
        }

        public CachingMethod Method { get; private set; }

        public string CacheKey { get; set; }

        public string[] RelatedAreas { get; private set; }
    }
}

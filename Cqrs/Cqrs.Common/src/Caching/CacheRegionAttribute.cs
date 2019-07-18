using System;

namespace Cqrs.Caching
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheRegionAttribute : Attribute
    {
        protected CacheRegionAttribute()
        { }

        public CacheRegionAttribute(string regionName)
        {
            Ensure.NotNullOrWhiteSpace(regionName, "regionName");

            this.CacheRegion = regionName;
        }

        public string CacheRegion { get; set; }
    }
}

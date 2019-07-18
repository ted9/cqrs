using System.Collections.Generic;
using System.Configuration;

namespace Cqrs.Caching.WebCache
{
    internal class WebCacheProvider : ICacheProvider
    {
        private readonly Dictionary<string, ICache> caches;

        public WebCacheProvider()
            : this(WebCacheSectionHandler.SectionName)
        { }

        public WebCacheProvider(string sectionKey)
        {
            caches = new Dictionary<string, ICache>();

            var list = ConfigurationManager.GetSection(sectionKey) as WebCacheConfig[];
            if (list != null) {
                foreach (WebCacheConfig cache in list) {
                    caches.Add(cache.Region, new WebCache(cache.Region, cache.Properties));
                }
            }
        }

        public ICache BuildCache(string regionName, IDictionary<string, string> properties)
        {
            ICache result;
            if (caches.TryGetValue(regionName, out result)) {
                return result;
            }

            return new WebCache(regionName, properties);
        }

    }
}

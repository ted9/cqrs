using System.Collections.Generic;

namespace Cqrs.Caching.WebCache
{
    internal class WebCacheConfig
    {
        private readonly Dictionary<string, string> properties;
        private readonly string regionName;


        /// <summary>
        /// build a configuration
        /// </summary>
        public WebCacheConfig(string region, string expiration, string priority)
        {
            regionName = region;

            properties = new Dictionary<string, string> { { "expiration", expiration }, { "priority", priority } };
        }


        public string Region
        {
            get { return regionName; }
        }


        public IDictionary<string, string> Properties
        {
            get { return properties; }
        }
    }
}

using System.Collections.Generic;
using System.Configuration;
using System.Xml;


namespace Cqrs.Caching.WebCache
{
    public class WebCacheSectionHandler : IConfigurationSectionHandler
    {
        internal const string SectionName = "thinkcache-configuration";

        public object Create(object parent, object configContext, XmlNode section)
        {
            var caches = new List<WebCacheConfig>();

            XmlNodeList nodes = section.SelectNodes("cache");

            foreach (XmlNode node in nodes) {
                string region = null;
                string expiration = null;
                string priority = "3";

                XmlAttribute r = node.Attributes["region"];
                XmlAttribute e = node.Attributes["expiration"];
                XmlAttribute p = node.Attributes["priority"];

                if (r != null) {
                    region = r.Value;
                }

                if (e != null) {
                    expiration = e.Value;
                }

                if (p != null) {
                    priority = p.Value;
                }

                if (region != null && expiration != null) {
                    caches.Add(new WebCacheConfig(region, expiration, priority));
                }
            }

            return caches.ToArray();
        }

    }
}

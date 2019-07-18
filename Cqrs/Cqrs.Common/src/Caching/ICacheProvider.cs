using System.Collections.Generic;
using Cqrs.Caching.WebCache;
using Cqrs.Components;

namespace Cqrs.Caching
{
    [RequiredComponent(typeof(WebCacheProvider))]
    public interface ICacheProvider
    {
        ICache BuildCache(string regionName, IDictionary<string, string> properties);
    }
}

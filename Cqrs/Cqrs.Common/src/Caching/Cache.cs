using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using Cqrs.Components;


namespace Cqrs.Caching
{
    public sealed class Cache
    {
        private readonly static Cache _instance = new Cache();

        public static Cache Current
        {
            get { return _instance; }
        }

        public static void SetProvider(Func<ICacheProvider> provider)
        {
            SetProvider(provider, null);
        }


        static ICacheProvider _cacheProvider = null;
        static IDictionary<string, string> _cacheProperties = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        public static void SetProvider(Func<ICacheProvider> provider, IDictionary<string, string> cacheProperties)
        {
            Ensure.NotNull(provider, "provider");
            _cacheProvider = provider.Invoke();
            cacheProperties.ForEach(kvp => {
                _cacheProperties[kvp.Key] = kvp.Value;
            });
        }

        #region
        private Cache()
        { }

        private ICacheProvider CacheProvider
        {
            get
            {
                if (_cacheProvider != null)
                    return _cacheProvider;

                return Interlocked.CompareExchange<ICacheProvider>(ref _cacheProvider, GetProvider(), null);
            }
        }

        
        private ICacheProvider GetProvider()
        {
            if (!ObjectContainer.Instance.IsRegistered<ICacheProvider>()) {
                return new WebCache.WebCacheProvider();
            }

            return ObjectContainer.Instance.Resolve<ICacheProvider>();
        }

        private ICache BuildCache(string regionName)
        {
            CacheProvider.BuildCache(regionName, _cacheProperties);
            return null;
        }

        readonly ConcurrentDictionary<string, ICache> _caches = new ConcurrentDictionary<string, ICache>(StringComparer.CurrentCultureIgnoreCase);
        private ICache GetCacheByRegion(string regionName)
        {
            return _caches.GetOrAdd(regionName, BuildCache);
        }
        #endregion

        public const string CacheRegion = "ThinkCache";

        public void Put(string key, object value)
        {
            Put(CacheRegion, key, value);
        }

        public void Put(string region, string key, object value)
        {
            GetCacheByRegion(region).Put(key, value);
        }

        public object Get(string key)
        {
            return Get(CacheRegion, key);
        }
        public object Get(string region, string key)
        {
            return GetCacheByRegion(region).Get(key);
        }
        public void Remove(string key)
        {
            Remove(CacheRegion, key);
        }
        public void Remove(string region, string key)
        {
            GetCacheByRegion(region).Remove(key);
        }
        public void Evict(string region)
        {
            GetCacheByRegion(region).Clear();

            _caches.Remove(region);
        }
    }
}

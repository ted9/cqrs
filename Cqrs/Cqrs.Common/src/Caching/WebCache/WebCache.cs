using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Cqrs.Caching.WebCache
{
    internal sealed class WebCache : ICache
    {
        public WebCache(string region, IDictionary<string, string> properties)
        {
            this.RegionName = region;
            this.Expiration = GetExpiration(properties);
            this.Priority = GetPriority(properties);

            cache = HttpRuntime.Cache;
            rootCacheKey = GenerateRootCacheKey();
            StoreRootCacheKey();
        }

        #region
        private static TimeSpan GetExpiration(IDictionary<string, string> props)
        {
            TimeSpan expiration = TimeSpan.FromSeconds(300);
            string expirationString;
            if (props != null && props.TryGetValue("expiration", out expirationString)) {
                expiration = TimeSpan.FromSeconds(expirationString.ToInt(300));
            }

            return expiration;
        }

        private static CacheItemPriority GetPriority(IDictionary<string, string> props)
        {
            CacheItemPriority result = CacheItemPriority.Default;
            string priorityString;
            if (props != null && props.TryGetValue("priority", out priorityString)) {
                result = ConvertCacheItemPriorityFromXmlString(priorityString);
            }
            return result;
        }

        private static CacheItemPriority ConvertCacheItemPriorityFromXmlString(string priorityString)
        {
            if (string.IsNullOrEmpty(priorityString)) {
                return CacheItemPriority.Default;
            }

            var ps = priorityString.Trim().ToLowerInvariant();

            if (ps.IsNumeric()) {
                int priorityAsInt = int.Parse(ps);
                if (priorityAsInt >= 1 && priorityAsInt <= 6) {
                    return (CacheItemPriority)priorityAsInt;
                }
            }

            switch (ps) {
                case "abovenormal":
                    return CacheItemPriority.AboveNormal;
                case "belownormal":
                    return CacheItemPriority.BelowNormal;
                case "high":
                    return CacheItemPriority.High;
                case "low":
                    return CacheItemPriority.Low;
                case "normal":
                    return CacheItemPriority.Normal;
                case "notremovable":
                    return CacheItemPriority.NotRemovable;
                default:
                    return CacheItemPriority.Default;
            }
        }
        #endregion

        private const string CacheKeyPrefix = "ThinkCache:";
        
        private readonly System.Web.Caching.Cache cache;
        private readonly string rootCacheKey;
        private bool rootCacheKeyStored;


        public TimeSpan Expiration { get; set; }

        public CacheItemPriority Priority { get; set; }

        private string GetCacheKey(string key)
        {
            return String.Concat(CacheKeyPrefix, RegionName, ":", key, "@", key.GetHashCode());
        }


        #region ICache 成员

        public object Get(string key)
        {
            Ensure.NotNullOrWhiteSpace(key, "key");

            string cacheKey = GetCacheKey(key);

            object obj = cache.Get(cacheKey);
            if (obj == null)
                return null;

            var de = (DictionaryEntry)obj;
            if (key.Equals(de.Key)) {
                return de.Value;
            }
            else {
                return null;
            }
        }

        public void Put(string key, object value)
        {
            Ensure.NotNullOrWhiteSpace(key, "key");
            Ensure.NotNull(value, "value");

            string cacheKey = GetCacheKey(key);
            if (cache[cacheKey] != null) {

                cache.Remove(cacheKey);
            }

            if (!rootCacheKeyStored) {
                StoreRootCacheKey();
            }

            cache.Insert(cacheKey,
                new DictionaryEntry(key, value),
                new CacheDependency(null, new[] { rootCacheKey }),
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                this.Expiration,
                this.Priority,
                null);
        }

        public void Remove(string key)
        {
            Ensure.NotNullOrWhiteSpace(key, "key");

            string cacheKey = GetCacheKey(key);
            cache.Remove(cacheKey);
        }

        public void Clear()
        {
            RemoveRootCacheKey();
            StoreRootCacheKey();
        }

        public void Destroy()
        {
            Clear();
        }

        public string RegionName
        {
            get;
            private set;
        }

        #endregion


        private string GenerateRootCacheKey()
        {
            return GetCacheKey(Guid.NewGuid().ToString());
        }

        private void RootCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            rootCacheKeyStored = false;
        }

        private void StoreRootCacheKey()
        {
            rootCacheKeyStored = true;
            cache.Add(
                rootCacheKey,
                rootCacheKey,
                null,
                System.Web.Caching.Cache.NoAbsoluteExpiration,
                System.Web.Caching.Cache.NoSlidingExpiration,
                CacheItemPriority.Default,
                RootCacheItemRemoved);
        }

        private void RemoveRootCacheKey()
        {
            cache.Remove(rootCacheKey);
        }
    }
}

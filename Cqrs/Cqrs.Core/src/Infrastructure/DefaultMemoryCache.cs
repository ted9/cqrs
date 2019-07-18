using System;
using System.Configuration;
using System.Reflection;

using Cqrs.Caching;
using Cqrs.Infrastructure.Serialization;


namespace Cqrs.Infrastructure
{
    /// <summary>
    /// 通过ThinkLib.Cache的配置实现的缓存。
    /// </summary>
    public sealed class DefaultMemoryCache : IMemoryCache
    {       
        internal readonly static IMemoryCache Instance = new DefaultMemoryCache();
        private DefaultMemoryCache()
        { }

        private readonly IBinarySerializer _serializer;
        private readonly bool _enabled;
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public DefaultMemoryCache(IBinarySerializer serializer)
        {
            this._serializer = serializer;
            this._enabled = ConfigurationManager.AppSettings["thinkcfg.caching_enabled"].Safe("false").ToBoolean();
        }
        /// <summary>
        /// 从缓存中获取该类型的实例。
        /// </summary>
        public object Get(Type type, object key)
        {
            if (!_enabled) return null;

            Ensure.NotNull(type, "type");
            Ensure.NotNull(key, "key");


            string cacheRegion = GetCacheRegion(type);
            string cacheKey = BuildCacheKey(type, key);

            object data = null;
            lock (cacheKey) {
                data = Cache.Current.Get(cacheRegion, cacheKey);
            }
            if (data == null)
                return null;

            return _serializer.Deserialize((byte[])data);
        }
        /// <summary>
        /// 设置实例到缓存
        /// </summary>
        public void Set(object entity, object key)
        {
            if (!_enabled) return;

            Ensure.NotNull(entity, "entity");
            Ensure.NotNull(key, "key");

            var type = entity.GetType();

            string cacheRegion = GetCacheRegion(type);
            string cacheKey = BuildCacheKey(type, key);

            var data = _serializer.Serialize(entity);

            lock (cacheKey) {
                Cache.Current.Put(cacheRegion, cacheKey, data);
            }
        }
        /// <summary>
        /// 从缓存中移除
        /// </summary>
        public void Remove(Type type, object key)
        {
            if (!_enabled) return;

            Ensure.NotNull(type, "type");
            Ensure.NotNull(key, "key");

            string cacheRegion = GetCacheRegion(type);
            string cacheKey = BuildCacheKey(type, key);

            lock (cacheKey) {
                Cache.Current.Remove(cacheRegion, cacheKey);
            }
        }


        private static string GetCacheRegion(Type type)
        {
            var attr = type.GetAttribute<CacheRegionAttribute>(false);
            if (attr == null)
                return "ThinkCache";

            return attr.CacheRegion;
        }
        private static string BuildCacheKey(Type type, object key)
        {
            return string.Format("Entity:{0}:{1}", type.FullName, key.ToString());
        }
    }
}

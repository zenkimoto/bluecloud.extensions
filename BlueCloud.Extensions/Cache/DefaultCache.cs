using System;
using System.Runtime.Caching;

namespace BlueCloud.Extensions.Cache
{
    /// <summary>
    /// Default cache utilizing System.Runtime.Caching.MemoryCache
    /// </summary>
    public class DefaultCache<T> : ICacheable<T>
    {
        /// <summary>
        /// Get cached object with specified key.
        /// </summary>
        /// <returns>Cached object</returns>
        /// <param name="key">Key</param>
        public T Get(string key)
        {
            ObjectCache cache = MemoryCache.Default;
            return (T)cache[key];
        }

        /// <summary>
        /// Caches an object with specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Set(string key, T value)
        {
            ObjectCache cache = MemoryCache.Default;

            CacheItemPolicy policy = new CacheItemPolicy
            {
                SlidingExpiration = new TimeSpan(4, 0, 0)
            };

            cache.Set(key, value, policy);
        }
    }
}

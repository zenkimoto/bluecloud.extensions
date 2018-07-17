using System;
using System.Runtime.Caching;

namespace BlueCloud.Extensions.Cache
{
    /// <summary>
    /// Default cache utilizing System.Runtime.Caching.MemoryCache
    /// </summary>
    public class DefaultCache<T> : ICacheable<T>, IDisposable
    {
        private readonly MemoryCache cache = new MemoryCache("DbProperties");

        public TimeSpan SlidingExpiration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/> class.
        /// </summary>
        /// <param name="slidingExpiration">Sliding Expiration</param>
        public DefaultCache(TimeSpan slidingExpiration)
        {
            SlidingExpiration = slidingExpiration;
        }


        /// <summary>
        /// Releases all resource used by the <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:BlueCloud.Extensions.Cache.DefaultCache`1"/> was occupying.</remarks>
        public void Dispose()
        {
            cache.Dispose();
        }


        /// <summary>
        /// Get cached object with specified key.
        /// </summary>
        /// <returns>Cached object</returns>
        /// <param name="key">Key</param>
        public T Get(string key)
        {
            return (T)cache[key];
        }


        /// <summary>
        /// Caches an object with specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Set(string key, T value)
        {
            var policy = new CacheItemPolicy
            {
                SlidingExpiration = SlidingExpiration
            };

            cache.Set(key, value, policy);
        }
    }
}

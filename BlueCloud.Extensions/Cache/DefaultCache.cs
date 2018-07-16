using System;
using System.Runtime.Caching;

namespace BlueCloud.Extensions.Cache
{
    public class DefaultCache<T> : ICacheable<T>
    {
        public T Get(string key)
        {
            ObjectCache cache = MemoryCache.Default;
            return (T)cache[key];
        }

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

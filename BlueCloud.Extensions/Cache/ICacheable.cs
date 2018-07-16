namespace BlueCloud.Extensions.Cache
{
    /// <summary>
    /// ICacheable Interface used for caching database field to object property mappings.
    /// Customize caching with your own implementation.
    /// </summary>
    public interface ICacheable<T>
    {
        /// <summary>
        /// Get cached object with specified key.
        /// </summary>
        /// <returns>Cached object</returns>
        /// <param name="key">Key</param>
        T Get(string key);

        /// <summary>
        /// Caches an object with specified key.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        void Set(string key, T value);
    }
}

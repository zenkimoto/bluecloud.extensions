namespace BlueCloud.Extensions.Cache
{
    public interface ICacheable<T>
    {
        T Get(string key);
        void Set(string key, T value);
    }
}

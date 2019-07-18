
namespace Cqrs.Caching
{
    public interface ICache
    {
        object Get(string key);

        void Put(string key, object value);

        void Remove(string key);

        void Clear();

        void Destroy();

        string RegionName { get; }
    }
}

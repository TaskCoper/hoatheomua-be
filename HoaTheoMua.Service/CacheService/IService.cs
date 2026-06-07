namespace HoaTheoMua.Service.CacheService;

public interface IService
{
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    bool TryGet<T>(string key, out T value);
    void Remove(string key);
}
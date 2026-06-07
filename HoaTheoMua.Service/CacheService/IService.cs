namespace HoaTheoMua.Service.CacheService;

public interface IService
{
    Task Set<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> TryGet<T>(string key);
    Task Remove(string key);
}
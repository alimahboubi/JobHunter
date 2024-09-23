using System.Text.Json;
using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.Caching.Distributed;

namespace JobHunter.Infrastructure.Cache.Redis;

public class RedisCacheService(IDistributedCache redisCache) : ICacheService
{
    public T? Get<T>(string key)
    {
        var serializedData= redisCache.GetString(key) ;
        if (string.IsNullOrWhiteSpace(serializedData)) return default;
        return JsonSerializer.Deserialize<T>(serializedData);
    }

    public void Set<T>(string key, T value, TimeSpan duration)
    {
        var serializedData = JsonSerializer.Serialize(value);
        redisCache.SetString(key, serializedData, new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow= duration
        });
    }

    public void Remove(string key)
    {
        redisCache.Remove(key);
    }
}
using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace JobHunter.Infrastructure.Cache.InMemory;

public class InMemoryCacheService(IMemoryCache memoryCache) : ICacheService
{
    public T? Get<T>(string key)
    {
        return memoryCache.TryGetValue(key, out T value) ? value : default;
    }

    public void Set<T>(string key, T value, TimeSpan duration)
    {
        memoryCache.Set(key, value, duration);
    }

    public void Remove(string key)
    {
        memoryCache.Remove(key);
    }
}
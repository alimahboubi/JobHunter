namespace JobHunter.Application.Abstraction.Cache;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan duration);
    void Remove(string key);
}
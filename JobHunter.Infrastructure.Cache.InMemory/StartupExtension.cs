using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure.Cache.InMemory;

public static class StartupExtension
{
    public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        return services;
    }
}
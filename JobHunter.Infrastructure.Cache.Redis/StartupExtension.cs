using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace JobHunter.Infrastructure.Cache.Redis;

public static class StartupExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IConnectionMultiplexer>(x =>
            ConnectionMultiplexer.Connect(connectionString));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}
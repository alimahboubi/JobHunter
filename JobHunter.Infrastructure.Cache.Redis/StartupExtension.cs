using JobHunter.Application.Abstraction.Cache;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace JobHunter.Infrastructure.Cache.Redis;

public static class StartupExtension
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services,
        RedisConfigurations redisConfigurations)
    {
        var configurationOptions = new ConfigurationOptions()
        {
            AbortOnConnectFail = false,
            ConnectTimeout = redisConfigurations.Timeout,
            Ssl = false
        };
        configurationOptions.EndPoints.Add(redisConfigurations.Host, redisConfigurations.Port);
        services.AddSingleton<IConnectionMultiplexer>(x =>
            ConnectionMultiplexer.Connect(configurationOptions));

        services.AddStackExchangeRedisCache(options => { options.ConfigurationOptions = configurationOptions; });
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}
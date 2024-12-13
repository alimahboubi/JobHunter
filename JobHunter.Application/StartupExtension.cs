using System.Reflection;
using JobHunter.Application.Mapping;
using JobHunter.Application.Services;
using JobHunter.Domain.Job.Services;
using JobHunter.Domain.User.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Application;

public static class StartupExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>()
            .AddScoped<IUserService, UserService>();
        return services;
    }
    
    public static IServiceCollection AddAutoMapperService(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserMappingProfile).Assembly);
        return services;
    }
}
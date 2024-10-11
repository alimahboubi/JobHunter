using JobHunter.Application.Services;
using JobHunter.Domain.Job.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Application;

public static class StartupExtension
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJobService, JobService>();
        return services;
    }
}
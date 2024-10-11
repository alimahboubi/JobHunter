using JobHunter.Domain.Job.Repositories;
using JobHunter.Infrastructure.Persistent.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure.Persistent.Postgres;

public static class StartupExtension
{
    public static IServiceCollection AddJobHunterDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<JobHunterDbContext>(dbOption => { dbOption.UseNpgsql(connectionString); });
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IProceedJobCheckpointRepository, ProceedJobCheckpointRepository>();
        return services;
    }
}
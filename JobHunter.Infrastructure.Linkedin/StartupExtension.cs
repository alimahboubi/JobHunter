using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure.Linkedin;

public static class StartupExtension
{
    public static IServiceCollection AddCrawlerService(this IServiceCollection services)
    {
        services.AddSingleton<JobSearchCrawler>();
        services.AddSingleton<IJobCrawlerService, JobCrawlerService>();
        return services;
    }

}
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Linkedin.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure.Linkedin;

public static class StartupExtension
{
    public static IServiceCollection AddCrawlerService(this IServiceCollection services)
    {
        services.AddSingleton<JobSearchCrawler>();
        services.AddSingleton<JobDescriptionCrawler>();
        services.AddSingleton<IJobCrawlerService, JobCrawlerService>();
        return services;
    }

    public static IServiceCollection AddLinkedinHttpClient(this IServiceCollection services, LinkedinConfiguration configuration)
    {
        services.AddHttpClient(nameof(LinkedinConfiguration),
            client =>
            {
                client.BaseAddress = new Uri(configuration.JobSearchBaseUrl);
                client.DefaultRequestHeaders.Add("Assistant-Agent",configuration.UserAgent);
            });
        return services;
    }
}
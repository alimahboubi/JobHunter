using JobHunter.Domain.Job.Repositories;
using JobHunter.Domain.Job.Services;
using JobHunter.Infrastructure.Analyzer.OpenAi.Configurations;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

namespace JobHunter.Infrastructure.Analyzer.OpenAi;

public static class StartupExtension
{
    public static IServiceCollection AddOpenAiService(this IServiceCollection services,
        OpenAiConfigurations configurations)
    {
        AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);
        OpenAIClient openAiClient = new(configurations.APIKey);
        services.AddSingleton(openAiClient);
        services.AddScoped<IJobAnalyzerService, GptJobDescriptionMatchAnalyzer>();
        return services;
    }
}
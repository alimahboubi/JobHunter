using System.Text.Json;
using JobHunter.Domain.Job.Configurations;

namespace JobHunter.Worker;

public static class StartupExtension
{
    public static IServiceCollection AddUserConfigurations(this IServiceCollection services,
        string? userConfigurationFilePath)
    {
        if (string.IsNullOrWhiteSpace(userConfigurationFilePath))
        {
            var currentDirectory = Environment.CurrentDirectory;
            var userConfigurationDirectoryPath = Path.Combine(currentDirectory, "UsersConfigurations");
            userConfigurationFilePath = Path.Combine(userConfigurationDirectoryPath, "UsersConfigurations.json");
        }

        using var sr = new StreamReader(userConfigurationFilePath);
        var configStr = sr.ReadToEnd();
        var userConfig = JsonSerializer.Deserialize<UsersConfigurations>(configStr);
        services.AddSingleton(userConfig);
        return services;
    }
}
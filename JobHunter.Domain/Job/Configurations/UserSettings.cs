namespace JobHunter.Domain.Job.Configurations;

public class UserSettings
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string ProxyServerUrl { get; set; }
    public JobTargetSettings JobTargetSettings { get; set; }
}
namespace JobHunter.Domain.Job.Configurations;

public class ProfileConfigurations
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Guid Id { get; set; }
    public string ProxyServerUrl { get; set; }
    public string TextResumePath { get; set; }
    public JobTargetSettings JobTargetSettings { get; set; }
}
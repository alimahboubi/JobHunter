namespace JobHunter.Infrastructure.Linkedin.Configurations;

public class LinkedinConfiguration
{
    public string JobSearchBaseUrl { get; set; }
    public int TimeIntervalSeconds { get; set; }
    public string UserAgent { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
namespace JobHunter.Infrastructure.Cache.Redis;

public class RedisConfigurations
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Password { get; set; }
    public int Timeout { get; set; }
}
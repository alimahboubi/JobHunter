namespace JobHunter.Infrastructure.Linkedin.Exceptions;

public class JobDescriptionCrawlerException(string url) : Exception($"Failed to fetch description of {url}");
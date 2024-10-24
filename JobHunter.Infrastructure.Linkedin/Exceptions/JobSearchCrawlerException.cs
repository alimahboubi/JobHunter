namespace JobHunter.Infrastructure.Linkedin.Exceptions;

public class JobSearchCrawlerException(string location) : Exception($"Failed to fetch job for {location}");
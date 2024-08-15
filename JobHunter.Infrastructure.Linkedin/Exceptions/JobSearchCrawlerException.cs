namespace JobHunter.Infrastructure.Linkedin.Exceptions;

public class JobSearchCrawlerException(string location) : Exception($"Failed to fetch job this location {location}");
namespace JobHunter.Infrastructure.Linkedin.Exceptions;

public class JobUrlException(string? url=null) : Exception("Job URL node not found.");
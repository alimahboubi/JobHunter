namespace JobHunter.Domain.Job.Exceptions;

public class CalculateMatchPercentageException():Exception("Failed to parse the match percentage from the assistant's response.");
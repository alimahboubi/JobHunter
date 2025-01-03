namespace JobHunter.Domain.Job.Services;

public interface IJobAnalyzerService
{
    Task<float> AnalyzeJob(string jobTitle, string? jobDescription, string? resumeContent, CancellationToken ct);
}
namespace JobHunter.Domain.Job.Services;

public interface IJobAnalysisBackgroundService
{
    Task Execute(CancellationToken cancellationToken);
}
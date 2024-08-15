namespace JobHunter.Domain.Job.Services;

public interface IBackgroundJobService
{
    Task Execute(CancellationToken cancellationToken);
}
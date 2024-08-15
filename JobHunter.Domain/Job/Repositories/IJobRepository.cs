namespace JobHunter.Domain.Job.Repositories;

public interface IJobRepository
{
    Task<bool> IsJobExistAsync(string sourceId, CancellationToken ct = default);
    Task AddAsync(Entities.Job job, CancellationToken ct = default);
}
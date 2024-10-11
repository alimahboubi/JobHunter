using JobHunter.Domain.Job.Entities;

namespace JobHunter.Domain.Job.Repositories;

public interface IProceedJobCheckpointRepository
{
    Task<ProceedJobCheckpoint?> GetAsync(string serviceName, CancellationToken ct = default);
    Task AddOrUpdate(string serviceName, int checkpoint, CancellationToken cancellationToken = default);
}
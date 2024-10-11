using System.ComponentModel;
using JobHunter.Domain.Job.Dto;

namespace JobHunter.Domain.Job.Repositories;

public interface IJobRepository
{
    Task<bool> IsJobExistAsync(string sourceId, CancellationToken ct = default);
    Task AddAsync(Entities.Job job, CancellationToken ct = default);
    Task<(List<Entities.Job>, int count)> GetJobs(GetPaginatedJobRequestDto requestDto, CancellationToken ct);
    Task<List<Entities.Job>> GetUnprocessedJobsAfterCheckpoint(int limit,int checkPoint, CancellationToken ct);
    Task<bool> RemoveById(int id, CancellationToken ct);
    
    Task<bool> UpdateAppliedState(int id, bool state, CancellationToken ct);
    Task<bool> UpdateMatchAccuracy(int id, float accuracy, CancellationToken ct);
}
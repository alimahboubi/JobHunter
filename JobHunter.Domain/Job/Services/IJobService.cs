using JobHunter.Domain.Job.Dto;

namespace JobHunter.Domain.Job.Services;

public interface IJobService
{
    Task<PaginationResultDto<GetPaginatedJobResponseDto>> GetJob(GetPaginatedJobRequestDto requestDto, CancellationToken ct);
    Task<bool> RemoveById(int id, CancellationToken ct);
    Task<bool> UpdateAppliedState(int id, bool state, CancellationToken ct);
    Task AddJobs(List<JobResultDto> jobs, Guid userId, CancellationToken ct);
}
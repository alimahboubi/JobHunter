using JobHunter.Domain.Job.Dto;

namespace JobHunter.Domain.Job.Services;

public interface IJobCrawlerService
{
    Task<List<JobResultDto>> GetJobPositions(TargetPositionDto targetPositionDto, CancellationToken ct= default);
}
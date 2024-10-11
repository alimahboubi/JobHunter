using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Repositories;
using JobHunter.Domain.Job.Services;

namespace JobHunter.Application.Services;

public class JobService(IJobRepository jobRepository) : IJobService
{
    public async Task<PaginationResultDto<GetPaginatedJobResponseDto>> GetJob(GetPaginatedJobRequestDto requestDto,
        CancellationToken ct)
    {
        var result = await jobRepository.GetJobs(requestDto, ct);
        var resultDto = result.Item1.Select(e => new GetPaginatedJobResponseDto()
        {
            Id = e.Id,
            Title = e.Title,
            Company = e.Company,
            Location = e.Location,
            Url = e.Url,
            PostedDate = e.PostedDate,
            IsApplied = e.IsApplied,
            SeniorityLevel = null,
            MatchAccuracy = e.MatchAccuracy
        }).ToList();
        return new PaginationResultDto<GetPaginatedJobResponseDto>
        {
            Data = resultDto,
            CurrentPage = requestDto.CurrentPage,
            TotalItems = result.count
        };
    }

    public async Task<bool> RemoveById(int id, CancellationToken ct)
    {
        return await jobRepository.RemoveById(id, ct);
    }

    public async Task<bool> UpdateAppliedState(int id, bool state, CancellationToken ct)
    {
        return await jobRepository.UpdateAppliedState(id, state, ct);
    }
}
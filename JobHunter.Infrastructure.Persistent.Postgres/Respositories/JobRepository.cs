using System.ComponentModel;
using JobHunter.Domain.Job.Dto;
using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Repositories;
using JobHunter.Infrastructure.Persistent.Postgres.Extensions;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Persistent.Postgres.Respositories;

public class JobRepository(JobHunterDbContext dbContext) : IJobRepository
{
    public async Task<bool> IsJobExistAsync(string sourceId, CancellationToken ct = default)
    {
        return await dbContext.Jobs.AnyAsync(e => e.SourceId == sourceId, ct);
    }

    public async Task AddAsync(Job job, CancellationToken ct = default)
    {
        await dbContext.Jobs.AddAsync(job, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<(List<Job>, int count)> GetJobs(GetPaginatedJobRequestDto requestDto, CancellationToken ct)
    {
        var currentPage = requestDto.CurrentPage < 1 ? 1 : requestDto.CurrentPage;
        var skip = (currentPage - 1) * requestDto.PageSize;
        var query = dbContext.Jobs.AsQueryable();
        if (requestDto.IsApplied is not null)
            query = query.Where(e => e.IsApplied == requestDto.IsApplied);
        if (requestDto.SortProperties.Any())
        {
            query = query.OrderBy(requestDto.SortProperties);
        }
        else
        {
            query = query.OrderByDescending(e => e.Id);
        }

        var count = await query.CountAsync(ct);
        var result = await query
            .AsTracking()
            .Skip(skip)
            .Take(requestDto.PageSize)
            .ToListAsync(ct);
        return (result, count);
    }

    public async Task<List<Job>> GetUnprocessedJobsAfterCheckpoint(int limit, int checkPoint, CancellationToken ct)
    {
        return await dbContext.Jobs
            .Where(e => e.Id > checkPoint && e.MatchAccuracy != null)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<bool> RemoveById(int id, CancellationToken ct)
    {
        return await dbContext.Jobs.Where(e => e.Id == id).ExecuteDeleteAsync(ct) > 0;
    }

    public async Task<bool> UpdateAppliedState(int id, bool state, CancellationToken ct)
    {
        return await dbContext.Jobs.Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.IsApplied, state), ct) > 0;
    }

    public async Task<bool> UpdateMatchAccuracy(int id, float accuracy, CancellationToken ct)
    {
        return await dbContext.Jobs.Where(e => e.Id == id)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.MatchAccuracy, accuracy), ct) > 0;
    }
}
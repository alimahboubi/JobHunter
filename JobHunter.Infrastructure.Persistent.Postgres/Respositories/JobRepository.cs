using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Repositories;
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
}
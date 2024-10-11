using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.Job.Repositories;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Persistent.Postgres.Repositories;

public class ProceedJobCheckpointRepository(JobHunterDbContext dbContext) : IProceedJobCheckpointRepository
{
    public async Task<ProceedJobCheckpoint?> GetAsync(string serviceName, CancellationToken ct = default)
    {
        return await dbContext.ProceedJobCheckpoints.FirstOrDefaultAsync(p => p.ServiceName == serviceName, ct);
    }

    public async Task AddOrUpdate(string serviceName, int checkpoint, CancellationToken cancellationToken = default)
    {
        var existingCheckpoint = await dbContext.ProceedJobCheckpoints
            .FirstOrDefaultAsync(p => p.ServiceName == serviceName, cancellationToken);

        if (existingCheckpoint != null)
        {
            // If it exists, update the record
            existingCheckpoint.Checkpoint = checkpoint;
            existingCheckpoint.TimeStamp = DateTime.UtcNow;
        }
        else
        {
            // If not, add a new record
            var proceedJobCheckpoint = new ProceedJobCheckpoint
            {
                ServiceName = serviceName,
                Checkpoint = 0,
                TimeStamp = DateTime.UtcNow
            };
            await dbContext.ProceedJobCheckpoints.AddAsync(proceedJobCheckpoint, cancellationToken);
        }

        // Save the changes
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
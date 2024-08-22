using JobHunter.Infrastructure.Persistent.Postgres;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Worker;

public class DbMigrator
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DbMigrator(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<JobHunterDbContext>();

        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
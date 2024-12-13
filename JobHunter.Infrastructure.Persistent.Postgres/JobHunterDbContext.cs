using System.Reflection;
using JobHunter.Domain.Job.Entities;
using JobHunter.Domain.User.Entities;
using JobHunter.Infrastructure.Persistent.Postgres.EntitiesConfigurations;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Persistent.Postgres;

public class JobHunterDbContext(DbContextOptions<JobHunterDbContext> options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<ProceedJobCheckpoint> ProceedJobCheckpoints { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobEntityConfigurations).Assembly);
    }
}
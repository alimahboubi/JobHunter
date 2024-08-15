using JobHunter.Domain.Job.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistent.Postgres.EntitiesConfigurations;

public class ProceedJobCheckPointEntityConfigurations : IEntityTypeConfiguration<ProceedJobCheckpoint>
{
    public void Configure(EntityTypeBuilder<ProceedJobCheckpoint> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Checkpoint).IsRequired();
        builder.Property(e => e.ServiceName).IsRequired();
        builder.Property(e => e.TimeStamp).IsRequired();
    }
}
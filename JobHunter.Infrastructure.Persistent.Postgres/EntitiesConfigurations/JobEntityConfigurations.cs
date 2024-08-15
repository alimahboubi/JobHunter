using JobHunter.Domain.Job.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistent.Postgres.EntitiesConfigurations;

public class JobEntityConfigurations : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Source).IsRequired();
        builder.Property(e => e.SourceId).IsRequired();
        builder.HasIndex(e => e.SourceId).IsUnique();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.Company).IsRequired();
        builder.Property(e => e.Url).IsRequired();
        builder.Property(e => e.Keywords).HasColumnType("jsonb");
    }
}
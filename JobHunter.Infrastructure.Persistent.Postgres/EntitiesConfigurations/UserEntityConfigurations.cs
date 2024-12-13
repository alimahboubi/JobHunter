using JobHunter.Domain.User.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistent.Postgres.EntitiesConfigurations;

public class UserEntityConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.IsEnabled).IsRequired();
        builder.Property(e => e.Resume).IsRequired();
        builder.Property(e => e.Name).IsRequired();
        builder.OwnsOne(e => e.JobTarget, jobTarget => { jobTarget.ToJson(); });
    }
}
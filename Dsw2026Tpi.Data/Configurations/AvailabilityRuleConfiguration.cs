using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
{
    public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
    {
        builder.ToTable("AvailabilityRules");
        builder.HasIndex(r => new { r.DoctorId, r.Year, r.Month, r.DayOfWeek, r.StartTime, r.EndTime })
                .IsUnique()
                .HasFilter("[Deleted] = 0");
    }
}

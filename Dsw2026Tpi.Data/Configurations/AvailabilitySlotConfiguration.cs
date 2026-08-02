using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");
        builder.HasIndex(s => new { s.DoctorId, s.SlotDate, s.StartTime })
            .IsUnique()
            .HasFilter("[Deleted] = 0");

        // Guarda el enum como texto (varchar) en lugar de nª
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
    }
}


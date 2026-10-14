using MaintenanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceSystem.Infrastructure.Persistence.Configurations;

public class MaintenanceTypeConfiguration : IEntityTypeConfiguration<MaintenanceType>
{
    public void Configure(EntityTypeBuilder<MaintenanceType> builder)
    {
        builder.ToTable("MaintenanceTypes", t =>
        {
            t.HasCheckConstraint("CK_MaintenanceTypes_Name", "LEN([Name]) > 0");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => m.Name)
            .IsUnique()
            .HasDatabaseName("IX_MaintenanceTypes_Name");

        builder.Property(m => m.Description)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(m => m.IsActive)
            .HasDatabaseName("IX_MaintenanceTypes_IsActive");

        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.Property(m => m.UpdatedAtUtc)
            .IsRequired(false);
    }
}


using MaintenanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceSystem.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        // Ignore CreatedAtUtc from Entity base class to match database-design.md Section 3.2
        builder.Ignore(r => r.CreatedAtUtc);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("IX_Roles_Name");

        builder.Property(r => r.Description)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(r => r.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}


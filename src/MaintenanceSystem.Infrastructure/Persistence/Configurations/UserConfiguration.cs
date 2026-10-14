using MaintenanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceSystem.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", t =>
        {
            t.HasCheckConstraint("CK_Users_FailedLoginCount", "[FailedLoginCount] >= 0");
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.UserName)
            .IsUnique()
            .HasDatabaseName("IX_Users_UserName");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.SecurityStamp)
            .IsRequired();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.FailedLoginCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutEndUtc)
            .IsRequired(false);

        builder.HasIndex(u => u.LockoutEndUtc)
            .HasDatabaseName("IX_Users_LockoutEndUtc");

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.Property(u => u.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(u => u.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}


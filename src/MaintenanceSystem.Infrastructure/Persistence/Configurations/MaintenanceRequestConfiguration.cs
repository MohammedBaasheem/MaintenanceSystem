using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MaintenanceSystem.Infrastructure.Persistence.Configurations;

public class MaintenanceRequestConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> builder)
    {
        builder.ToTable("MaintenanceRequests", t =>
        {
            t.HasCheckConstraint("CK_MaintenanceRequests_Priority", "[Priority] BETWEEN 0 AND 3");
            t.HasCheckConstraint("CK_MaintenanceRequests_Status", "[Status] BETWEEN 0 AND 5");
            t.HasCheckConstraint("CK_MaintenanceRequests_Title", "LEN([Title]) > 0");
            t.HasCheckConstraint("CK_MaintenanceRequests_Description", "LEN([Description]) > 0");
        });

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.Priority)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired()
            .HasDefaultValue(MaintenanceRequestStatus.New);

        builder.Property(m => m.MaintenanceTypeId)
            .IsRequired();

        builder.Property(m => m.CreatedByUserId)
            .IsRequired();

        builder.Property(m => m.AssignedTechnicianId)
            .IsRequired(false);

        builder.Property(m => m.RejectionReason)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAtUtc)
            .IsRequired();

        builder.Property(m => m.AssignedAtUtc)
            .IsRequired(false);

        builder.Property(m => m.ResolvedAtUtc)
            .IsRequired(false);

        builder.Property(m => m.ClosedAtUtc)
            .IsRequired(false);

        builder.Property(m => m.UpdatedAtUtc)
            .IsRequired(false);

        builder.HasIndex(m => m.CreatedByUserId)
            .HasDatabaseName("IX_MaintenanceRequests_CreatedByUserId");

        builder.HasIndex(m => m.AssignedTechnicianId)
            .HasDatabaseName("IX_MaintenanceRequests_AssignedTechnicianId");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_MaintenanceRequests_Status");

        builder.HasIndex(m => m.MaintenanceTypeId)
            .HasDatabaseName("IX_MaintenanceRequests_MaintenanceTypeId");

        builder.HasIndex(m => new { m.Status, m.AssignedTechnicianId })
            .HasDatabaseName("IX_MaintenanceRequests_Status_AssignedTechnicianId");

        builder.HasOne<MaintenanceType>()
            .WithMany()
            .HasForeignKey(m => m.MaintenanceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}


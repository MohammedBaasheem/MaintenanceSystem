using MaintenanceSystem.Domain.Common;
using MaintenanceSystem.Domain.Enums;
using MaintenanceSystem.Domain.Exceptions;

namespace MaintenanceSystem.Domain.Entities;

public class MaintenanceRequest : Entity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public MaintenancePriority Priority { get; private set; }
    public MaintenanceRequestStatus Status { get; private set; }
    public Guid MaintenanceTypeId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedTechnicianId { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? AssignedAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private MaintenanceRequest() { } // Required for EF Core

    public MaintenanceRequest(
        string title,
        string description,
        MaintenancePriority priority,
        Guid maintenanceTypeId,
        Guid createdByUserId)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        if (!Enum.IsDefined(typeof(MaintenancePriority), priority))
            throw new BusinessRuleViolationException("Invalid maintenance priority value.");

        if (maintenanceTypeId == Guid.Empty)
            throw new BusinessRuleViolationException("Maintenance type ID cannot be empty.");

        if (createdByUserId == Guid.Empty)
            throw new BusinessRuleViolationException("Created-by user ID cannot be empty.");

        Title = title.Trim();
        Description = description.Trim();
        Priority = priority;
        MaintenanceTypeId = maintenanceTypeId;
        CreatedByUserId = createdByUserId;
        Status = MaintenanceRequestStatus.New;
        AssignedTechnicianId = null;
        RejectionReason = null;
        AssignedAtUtc = null;
        ResolvedAtUtc = null;
        ClosedAtUtc = null;
        UpdatedAtUtc = null;
    }

    public void AssignTechnician(Guid technicianId, DateTime? utcNow = null)
    {
        if (Status != MaintenanceRequestStatus.New && Status != MaintenanceRequestStatus.Assigned)
        {
            throw new InvalidStateTransitionException(
                $"Cannot assign technician to request with status '{Status}'. Request must be in '{MaintenanceRequestStatus.New}' or '{MaintenanceRequestStatus.Assigned}' status.");
        }

        if (technicianId == Guid.Empty)
            throw new BusinessRuleViolationException("Assigned technician ID cannot be empty.");

        var now = utcNow ?? DateTime.UtcNow;
        AssignedTechnicianId = technicianId;
        AssignedAtUtc = now;
        Status = MaintenanceRequestStatus.Assigned;
        UpdatedAtUtc = now;
    }

    public void Start(DateTime? utcNow = null)
    {
        if (Status != MaintenanceRequestStatus.Assigned)
        {
            throw new InvalidStateTransitionException(
                $"Cannot start request with status '{Status}'. Request must be in '{MaintenanceRequestStatus.Assigned}' status.");
        }

        var now = utcNow ?? DateTime.UtcNow;
        Status = MaintenanceRequestStatus.InProgress;
        UpdatedAtUtc = now;
    }

    public void Resolve(DateTime? utcNow = null)
    {
        if (Status != MaintenanceRequestStatus.InProgress)
        {
            throw new InvalidStateTransitionException(
                $"Cannot resolve request with status '{Status}'. Request must be in '{MaintenanceRequestStatus.InProgress}' status.");
        }

        var now = utcNow ?? DateTime.UtcNow;
        Status = MaintenanceRequestStatus.Resolved;
        ResolvedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void Close(DateTime? utcNow = null)
    {
        if (Status != MaintenanceRequestStatus.Resolved)
        {
            throw new InvalidStateTransitionException(
                $"Cannot close request with status '{Status}'. Request must be in '{MaintenanceRequestStatus.Resolved}' status.");
        }

        var now = utcNow ?? DateTime.UtcNow;
        Status = MaintenanceRequestStatus.Closed;
        ClosedAtUtc = now;
        UpdatedAtUtc = now;
    }

    public void Reject(string reason, DateTime? utcNow = null)
    {
        if (Status != MaintenanceRequestStatus.New)
        {
            throw new InvalidStateTransitionException(
                $"Cannot reject request with status '{Status}'. Only requests with status '{MaintenanceRequestStatus.New}' can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleViolationException("Rejection reason cannot be empty.");

        var trimmedReason = reason.Trim();
        if (trimmedReason.Length > 500)
            throw new BusinessRuleViolationException("Rejection reason cannot exceed 500 characters.");

        var now = utcNow ?? DateTime.UtcNow;
        Status = MaintenanceRequestStatus.Rejected;
        RejectionReason = trimmedReason;
        UpdatedAtUtc = now;
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleViolationException("Title cannot be empty.");

        if (title.Trim().Length > 200)
            throw new BusinessRuleViolationException("Title cannot exceed 200 characters.");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleViolationException("Description cannot be empty.");

        if (description.Trim().Length > 2000)
            throw new BusinessRuleViolationException("Description cannot exceed 2000 characters.");
    }
}


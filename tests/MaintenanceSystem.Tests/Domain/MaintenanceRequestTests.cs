using FluentAssertions;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using MaintenanceSystem.Domain.Exceptions;
using Xunit;

namespace MaintenanceSystem.Tests.Domain;

public class MaintenanceRequestTests
{
    private static MaintenanceRequest CreateValidRequest(
        string title = "HVAC Unit Failure",
        string description = "Compressor is making loud buzzing noise and not cooling.",
        MaintenancePriority priority = MaintenancePriority.High,
        Guid? maintenanceTypeId = null,
        Guid? createdByUserId = null)
    {
        return new MaintenanceRequest(
            title,
            description,
            priority,
            maintenanceTypeId ?? Guid.NewGuid(),
            createdByUserId ?? Guid.NewGuid());
    }

    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeWithNewStatusAndNullAuditDates()
    {
        // Arrange
        var typeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var request = new MaintenanceRequest(
            "Broken Pipe",
            "Water leak under sink",
            MaintenancePriority.Critical,
            typeId,
            userId);

        // Assert
        request.Title.Should().Be("Broken Pipe");
        request.Description.Should().Be("Water leak under sink");
        request.Priority.Should().Be(MaintenancePriority.Critical);
        request.Status.Should().Be(MaintenanceRequestStatus.New);
        request.MaintenanceTypeId.Should().Be(typeId);
        request.CreatedByUserId.Should().Be(userId);
        request.AssignedTechnicianId.Should().BeNull();
        request.RejectionReason.Should().BeNull();
        request.AssignedAtUtc.Should().BeNull();
        request.ResolvedAtUtc.Should().BeNull();
        request.ClosedAtUtc.Should().BeNull();
        request.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ShouldThrowBusinessRuleViolationException(string? invalidTitle)
    {
        // Act
        Action act = () => new MaintenanceRequest(
            invalidTitle!,
            "Description",
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Title cannot be empty*");
    }

    [Fact]
    public void Constructor_WithTitleExceeding200Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var longTitle = new string('A', 201);

        // Act
        Action act = () => new MaintenanceRequest(
            longTitle,
            "Description",
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Title cannot exceed 200 characters*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyDescription_ShouldThrowBusinessRuleViolationException(string? invalidDesc)
    {
        // Act
        Action act = () => new MaintenanceRequest(
            "Valid Title",
            invalidDesc!,
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Description cannot be empty*");
    }

    [Fact]
    public void Constructor_WithDescriptionExceeding2000Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var longDesc = new string('D', 2001);

        // Act
        Action act = () => new MaintenanceRequest(
            "Valid Title",
            longDesc,
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Description cannot exceed 2000 characters*");
    }

    [Fact]
    public void Constructor_WithEmptyMaintenanceTypeId_ShouldThrowBusinessRuleViolationException()
    {
        // Act
        Action act = () => new MaintenanceRequest(
            "Valid Title",
            "Valid Description",
            MaintenancePriority.Medium,
            Guid.Empty,
            Guid.NewGuid());

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type ID cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyCreatedByUserId_ShouldThrowBusinessRuleViolationException()
    {
        // Act
        Action act = () => new MaintenanceRequest(
            "Valid Title",
            "Valid Description",
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.Empty);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Created-by user ID cannot be empty*");
    }

    [Fact]
    public void AssignTechnician_FromNewStatus_ShouldTransitionToAssignedAndRecordTimestamp()
    {
        // Arrange
        var request = CreateValidRequest();
        var technicianId = Guid.NewGuid();
        var utcNow = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        request.AssignTechnician(technicianId, utcNow);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.Assigned);
        request.AssignedTechnicianId.Should().Be(technicianId);
        request.AssignedAtUtc.Should().Be(utcNow);
        request.UpdatedAtUtc.Should().Be(utcNow);
    }

    [Fact]
    public void AssignTechnician_FromAssignedStatus_ShouldReassignTechnicianAndKeepAssignedStatus()
    {
        // Arrange
        var request = CreateValidRequest();
        var originalTechId = Guid.NewGuid();
        var initialTime = new DateTime(2026, 10, 15, 9, 0, 0, DateTimeKind.Utc);
        request.AssignTechnician(originalTechId, initialTime);

        var newTechId = Guid.NewGuid();
        var reassignTime = new DateTime(2026, 10, 15, 11, 0, 0, DateTimeKind.Utc);

        // Act
        request.AssignTechnician(newTechId, reassignTime);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.Assigned);
        request.AssignedTechnicianId.Should().Be(newTechId);
        request.AssignedAtUtc.Should().Be(reassignTime);
        request.UpdatedAtUtc.Should().Be(reassignTime);
    }

    [Fact]
    public void AssignTechnician_WithEmptyGuid_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        Action act = () => request.AssignTechnician(Guid.Empty);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Assigned technician ID cannot be empty*");
        request.Status.Should().Be(MaintenanceRequestStatus.New);
        request.AssignedTechnicianId.Should().BeNull();
    }

    [Fact]
    public void Start_FromAssignedStatus_ShouldTransitionToInProgress()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AssignTechnician(Guid.NewGuid());
        var startTime = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        request.Start(startTime);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.InProgress);
        request.UpdatedAtUtc.Should().Be(startTime);
    }

    [Fact]
    public void Resolve_FromInProgressStatus_ShouldTransitionToResolvedAndRecordTimestamp()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AssignTechnician(Guid.NewGuid());
        request.Start();
        var resolveTime = new DateTime(2026, 10, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        request.Resolve(resolveTime);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.Resolved);
        request.ResolvedAtUtc.Should().Be(resolveTime);
        request.UpdatedAtUtc.Should().Be(resolveTime);
    }

    [Fact]
    public void Close_FromResolvedStatus_ShouldTransitionToClosedAndRecordTimestamp()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AssignTechnician(Guid.NewGuid());
        request.Start();
        request.Resolve();
        var closeTime = new DateTime(2026, 10, 15, 16, 0, 0, DateTimeKind.Utc);

        // Act
        request.Close(closeTime);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.Closed);
        request.ClosedAtUtc.Should().Be(closeTime);
        request.UpdatedAtUtc.Should().Be(closeTime);
    }

    [Fact]
    public void Reject_FromNewStatus_ShouldTransitionToRejectedAndRecordReason()
    {
        // Arrange
        var request = CreateValidRequest();
        var rejectTime = new DateTime(2026, 10, 15, 8, 30, 0, DateTimeKind.Utc);
        const string reason = "Requested equipment is not within facility maintenance scope.";

        // Act
        request.Reject(reason, rejectTime);

        // Assert
        request.Status.Should().Be(MaintenanceRequestStatus.Rejected);
        request.RejectionReason.Should().Be(reason);
        request.UpdatedAtUtc.Should().Be(rejectTime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_WithEmptyReason_ShouldThrowBusinessRuleViolationException(string? invalidReason)
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        Action act = () => request.Reject(invalidReason!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Rejection reason cannot be empty*");
        request.Status.Should().Be(MaintenanceRequestStatus.New);
        request.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Reject_WithReasonExceeding500Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var request = CreateValidRequest();
        var longReason = new string('R', 501);

        // Act
        Action act = () => request.Reject(longReason);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Rejection reason cannot exceed 500 characters*");
        request.Status.Should().Be(MaintenanceRequestStatus.New);
        request.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Start_WhenStatusIsNew_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        Action act = () => request.Start();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot start request with status 'New'*");
        request.Status.Should().Be(MaintenanceRequestStatus.New);
        request.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Resolve_WhenStatusIsAssigned_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = CreateValidRequest();
        var techId = Guid.NewGuid();
        request.AssignTechnician(techId);

        // Act
        Action act = () => request.Resolve();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot resolve request with status 'Assigned'*");
        request.Status.Should().Be(MaintenanceRequestStatus.Assigned);
        request.AssignedTechnicianId.Should().Be(techId);
        request.ResolvedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Close_WhenStatusIsInProgress_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AssignTechnician(Guid.NewGuid());
        request.Start();

        // Act
        Action act = () => request.Close();

        // Assert
        act.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot close request with status 'InProgress'*");
        request.Status.Should().Be(MaintenanceRequestStatus.InProgress);
        request.ClosedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Reject_WhenStatusIsNotNew_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = CreateValidRequest();
        var techId = Guid.NewGuid();
        request.AssignTechnician(techId);

        // Act
        Action act = () => request.Reject("Invalid rejection after assignment");

        // Assert
        act.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Only requests with status 'New' can be rejected*");
        request.Status.Should().Be(MaintenanceRequestStatus.Assigned);
        request.AssignedTechnicianId.Should().Be(techId);
        request.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void ClosedStatus_WhenAttemptingAnyTransition_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = CreateValidRequest();
        var techId = Guid.NewGuid();
        request.AssignTechnician(techId);
        request.Start();
        request.Resolve();
        request.Close();

        // Act & Assert - AssignTechnician
        Action assign = () => request.AssignTechnician(Guid.NewGuid());
        assign.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot assign technician to request with status 'Closed'*");

        // Act & Assert - Start
        Action start = () => request.Start();
        start.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot start request with status 'Closed'*");

        // Act & Assert - Resolve
        Action resolve = () => request.Resolve();
        resolve.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot resolve request with status 'Closed'*");

        // Act & Assert - Close
        Action close = () => request.Close();
        close.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Cannot close request with status 'Closed'*");

        // Act & Assert - Reject
        Action reject = () => request.Reject("Reason");
        reject.Should().Throw<InvalidStateTransitionException>()
            .WithMessage("*Only requests with status 'New' can be rejected*");

        // State remains Closed
        request.Status.Should().Be(MaintenanceRequestStatus.Closed);
        request.AssignedTechnicianId.Should().Be(techId);
    }
}


using FluentAssertions;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.MaintenanceRequests.RejectMaintenanceRequest;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using MaintenanceSystem.Domain.Exceptions;
using Moq;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Handlers;

public class RejectMaintenanceRequestCommandHandlerTests
{
    private readonly Mock<IMaintenanceRequestRepository> _requestRepositoryMock = new();
    private readonly RejectMaintenanceRequestCommandHandler _handler;

    public RejectMaintenanceRequestCommandHandlerTests()
    {
        _handler = new RejectMaintenanceRequestCommandHandler(_requestRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceRequest?)null);

        var command = new RejectMaintenanceRequestCommand(nonExistentId, "Valid reason");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{nonExistentId}*");

        _requestRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestIsInNewStatus_ShouldRejectAndPersistUpdate()
    {
        // Arrange
        var request = new MaintenanceRequest(
            "Plumbing Leak",
            "Water leak in restroom",
            MaintenancePriority.Medium,
            Guid.NewGuid(),
            Guid.NewGuid());

        const string rejectionReason = "Duplicate ticket with #1042";

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        _requestRepositoryMock
            .Setup(r => r.UpdateAsync(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RejectMaintenanceRequestCommand(request.Id, rejectionReason);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(request.Id);
        result.Status.Should().Be(MaintenanceRequestStatus.Rejected);
        result.RejectionReason.Should().Be(rejectionReason);

        request.Status.Should().Be(MaintenanceRequestStatus.Rejected);
        request.RejectionReason.Should().Be(rejectionReason);

        _requestRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<MaintenanceRequest>(m => m.Id == request.Id && m.Status == MaintenanceRequestStatus.Rejected), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotInNewStatus_ShouldThrowInvalidStateTransitionException()
    {
        // Arrange
        var request = new MaintenanceRequest(
            "Elevator Malfunction",
            "Elevator door sensor stuck",
            MaintenancePriority.Critical,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assign a technician so status changes from New to Assigned
        request.AssignTechnician(Guid.NewGuid());

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);

        var command = new RejectMaintenanceRequestCommand(request.Id, "Attempting late rejection");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidStateTransitionException>()
            .WithMessage("*Only requests with status 'New' can be rejected*");

        _requestRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}


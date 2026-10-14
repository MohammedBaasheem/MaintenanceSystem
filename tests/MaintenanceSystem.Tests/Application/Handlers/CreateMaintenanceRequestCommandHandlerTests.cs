using FluentAssertions;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using Moq;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Handlers;

public class CreateMaintenanceRequestCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IMaintenanceTypeRepository> _maintenanceTypeRepositoryMock = new();
    private readonly Mock<IMaintenanceRequestRepository> _maintenanceRequestRepositoryMock = new();
    private readonly CreateMaintenanceRequestCommandHandler _handler;

    public CreateMaintenanceRequestCommandHandlerTests()
    {
        _handler = new CreateMaintenanceRequestCommandHandler(
            _currentUserServiceMock.Object,
            _maintenanceTypeRepositoryMock.Object,
            _maintenanceRequestRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);
        var command = new CreateMaintenanceRequestCommand(
            "HVAC Leak",
            "Coolant leaking",
            MaintenancePriority.High,
            Guid.NewGuid());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*المستخدم غير مصادق عليه*");

        _maintenanceRequestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMaintenanceTypeDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _maintenanceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MaintenanceType?)null);

        var command = new CreateMaintenanceRequestCommand(
            "HVAC Leak",
            "Coolant leaking",
            MaintenancePriority.High,
            typeId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{typeId}*");

        _maintenanceRequestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMaintenanceTypeIsInactive_ShouldThrowConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var maintenanceType = new MaintenanceType("HVAC", "Air Conditioning");
        maintenanceType.Deactivate();

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _maintenanceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(maintenanceType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceType);

        var command = new CreateMaintenanceRequestCommand(
            "HVAC Repair",
            "Air conditioner not turning on",
            MaintenancePriority.Medium,
            maintenanceType.Id);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"*{maintenanceType.Name}*معطّل*");

        _maintenanceRequestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldPersistWithNewStatusAndReturnResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var maintenanceType = new MaintenanceType("Electrical", "Wiring and circuits");

        _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
        _maintenanceTypeRepositoryMock
            .Setup(r => r.GetByIdAsync(maintenanceType.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(maintenanceType);

        MaintenanceRequest? savedRequest = null;
        _maintenanceRequestRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()))
            .Callback<MaintenanceRequest, CancellationToken>((req, _) => savedRequest = req)
            .Returns(Task.CompletedTask);

        var command = new CreateMaintenanceRequestCommand(
            "Circuit Breaker Trip",
            "Main breaker tripped on second floor",
            MaintenancePriority.Critical,
            maintenanceType.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Circuit Breaker Trip");
        result.Description.Should().Be("Main breaker tripped on second floor");
        result.Priority.Should().Be(MaintenancePriority.Critical);
        result.Status.Should().Be(MaintenanceRequestStatus.New);
        result.MaintenanceTypeId.Should().Be(maintenanceType.Id);
        result.CreatedByUserId.Should().Be(userId);

        savedRequest.Should().NotBeNull();
        savedRequest!.Status.Should().Be(MaintenanceRequestStatus.New);
        savedRequest.CreatedByUserId.Should().Be(userId);

        _maintenanceRequestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<MaintenanceRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}


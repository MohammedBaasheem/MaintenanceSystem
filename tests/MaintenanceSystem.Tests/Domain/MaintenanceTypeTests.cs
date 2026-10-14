using FluentAssertions;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Exceptions;
using Xunit;

namespace MaintenanceSystem.Tests.Domain;

public class MaintenanceTypeTests
{
    private static MaintenanceType CreateValidType(
        string name = "Electrical",
        string? description = "Wiring, breakers, switches, and fixtures.")
    {
        return new MaintenanceType(name, description);
    }

    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeCorrectly()
    {
        // Act
        var type = new MaintenanceType("  Plumbing  ", "  Pipes and drainage  ");

        // Assert
        type.Name.Should().Be("Plumbing");
        type.Description.Should().Be("Pipes and drainage");
        type.IsActive.Should().BeTrue();
        type.UpdatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyName_ShouldThrowBusinessRuleViolationException(string? invalidName)
    {
        // Act
        Action act = () => new MaintenanceType(invalidName!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type name cannot be empty*");
    }

    [Fact]
    public void Constructor_WithNameExceeding100Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var longName = new string('N', 101);

        // Act
        Action act = () => new MaintenanceType(longName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type name cannot exceed 100 characters*");
    }

    [Fact]
    public void Constructor_WithDescriptionExceeding500Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var longDesc = new string('D', 501);

        // Act
        Action act = () => new MaintenanceType("HVAC", longDesc);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type description cannot exceed 500 characters*");
    }

    [Fact]
    public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var type = CreateValidType();
        var utcNow = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);

        // Act
        type.Rename("Electrical & Automation", utcNow);

        // Assert
        type.Name.Should().Be("Electrical & Automation");
        type.UpdatedAtUtc.Should().Be(utcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithEmptyName_ShouldThrowBusinessRuleViolationException(string? invalidName)
    {
        // Arrange
        var type = CreateValidType();

        // Act
        Action act = () => type.Rename(invalidName!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type name cannot be empty*");
    }

    [Fact]
    public void Rename_WithNameExceeding100Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var type = CreateValidType();
        var longName = new string('T', 101);

        // Act
        Action act = () => type.Rename(longName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*Maintenance type name cannot exceed 100 characters*");
    }

    [Fact]
    public void Rename_WithSameName_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var type = CreateValidType("Plumbing");
        var initialTime = new DateTime(2026, 10, 15, 10, 0, 0, DateTimeKind.Utc);
        type.Rename("Plumbing Updated", initialTime);

        var subsequentTime = new DateTime(2026, 10, 15, 11, 0, 0, DateTimeKind.Utc);

        // Act
        type.Rename("  Plumbing Updated  ", subsequentTime);

        // Assert
        type.UpdatedAtUtc.Should().Be(initialTime);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalseAndRecordTimestamp()
    {
        // Arrange
        var type = CreateValidType();
        var utcNow = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        type.Deactivate(utcNow);

        // Assert
        type.IsActive.Should().BeFalse();
        type.UpdatedAtUtc.Should().Be(utcNow);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotModifyTimestamp()
    {
        // Arrange
        var type = CreateValidType();
        var firstDeactivateTime = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
        type.Deactivate(firstDeactivateTime);

        var secondDeactivateTime = new DateTime(2026, 10, 15, 13, 0, 0, DateTimeKind.Utc);

        // Act
        type.Deactivate(secondDeactivateTime);

        // Assert
        type.IsActive.Should().BeFalse();
        type.UpdatedAtUtc.Should().Be(firstDeactivateTime);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveTrueAndRecordTimestamp()
    {
        // Arrange
        var type = CreateValidType();
        type.Deactivate();

        var activateTime = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc);

        // Act
        type.Activate(activateTime);

        // Assert
        type.IsActive.Should().BeTrue();
        type.UpdatedAtUtc.Should().Be(activateTime);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotModifyTimestamp()
    {
        // Arrange
        var type = CreateValidType();
        var activateTime = new DateTime(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc);

        // Act
        type.Activate(activateTime);

        // Assert
        type.IsActive.Should().BeTrue();
        type.UpdatedAtUtc.Should().BeNull();
    }
}


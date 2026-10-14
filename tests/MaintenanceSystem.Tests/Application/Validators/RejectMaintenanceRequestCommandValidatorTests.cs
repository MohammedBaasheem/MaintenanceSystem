using FluentAssertions;
using FluentValidation.TestHelper;
using MaintenanceSystem.Application.Features.MaintenanceRequests.RejectMaintenanceRequest;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Validators;

public class RejectMaintenanceRequestCommandValidatorTests
{
    private readonly RejectMaintenanceRequestCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new RejectMaintenanceRequestCommand(Guid.NewGuid(), "Scope out of maintenance agreement");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRequestId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RejectMaintenanceRequestCommand(Guid.Empty, "Scope out of maintenance agreement");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.RequestId)
            .WithErrorMessage("RequestId cannot be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyReason_ShouldHaveValidationError(string? invalidReason)
    {
        // Arrange
        var command = new RejectMaintenanceRequestCommand(Guid.NewGuid(), invalidReason!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot be empty.");
    }

    [Fact]
    public void Validate_WithReasonExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longReason = new string('R', 501);
        var command = new RejectMaintenanceRequestCommand(Guid.NewGuid(), longReason);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 500 characters.");
    }
}


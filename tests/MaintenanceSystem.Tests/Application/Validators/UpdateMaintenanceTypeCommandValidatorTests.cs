using FluentAssertions;
using FluentValidation.TestHelper;
using MaintenanceSystem.Application.Features.MaintenanceTypes.UpdateMaintenanceType;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Validators;

public class UpdateMaintenanceTypeCommandValidatorTests
{
    private readonly UpdateMaintenanceTypeCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new UpdateMaintenanceTypeCommand(Guid.NewGuid(), "HVAC Maintenance", "Cooling systems");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateMaintenanceTypeCommand(Guid.Empty, "HVAC Maintenance", "Cooling systems");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("معرف نوع الصيانة مطلوب.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_ShouldHaveValidationError(string? invalidName)
    {
        // Arrange
        var command = new UpdateMaintenanceTypeCommand(Guid.NewGuid(), invalidName!, "Cooling systems");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("اسم نوع الصيانة مطلوب.");
    }

    [Fact]
    public void Validate_WithNameExceeding100Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('N', 101);
        var command = new UpdateMaintenanceTypeCommand(Guid.NewGuid(), longName, "Cooling systems");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("اسم نوع الصيانة لا يمكن أن يتجاوز 100 حرف.");
    }

    [Fact]
    public void Validate_WithDescriptionExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('D', 501);
        var command = new UpdateMaintenanceTypeCommand(Guid.NewGuid(), "Valid Name", longDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("وصف نوع الصيانة لا يمكن أن يتجاوز 500 حرف.");
    }
}


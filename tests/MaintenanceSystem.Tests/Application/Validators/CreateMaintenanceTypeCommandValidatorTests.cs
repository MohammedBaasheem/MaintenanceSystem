using FluentAssertions;
using FluentValidation.TestHelper;
using MaintenanceSystem.Application.Features.MaintenanceTypes.CreateMaintenanceType;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Validators;

public class CreateMaintenanceTypeCommandValidatorTests
{
    private readonly CreateMaintenanceTypeCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateMaintenanceTypeCommand("Electrical Repair", "Repairs for breakers and lighting");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_ShouldHaveValidationError(string? invalidName)
    {
        // Arrange
        var command = new CreateMaintenanceTypeCommand(invalidName!, "Valid description");

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
        var longName = new string('A', 101);
        var command = new CreateMaintenanceTypeCommand(longName, "Valid description");

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
        var command = new CreateMaintenanceTypeCommand("Plumbing", longDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("وصف نوع الصيانة لا يمكن أن يتجاوز 500 حرف.");
    }

    [Fact]
    public void Validate_WithNullDescription_ShouldBeValid()
    {
        // Arrange
        var command = new CreateMaintenanceTypeCommand("Plumbing", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}


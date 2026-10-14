using FluentAssertions;
using FluentValidation.TestHelper;
using MaintenanceSystem.Application.Features.MaintenanceRequests.CreateMaintenanceRequest;
using MaintenanceSystem.Domain.Enums;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Validators;

public class CreateMaintenanceRequestCommandValidatorTests
{
    private readonly CreateMaintenanceRequestCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new CreateMaintenanceRequestCommand(
            "Water Leak",
            "Pipe burst in basement",
            MaintenancePriority.High,
            Guid.NewGuid());

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
    public void Validate_WithEmptyTitle_ShouldHaveValidationError(string? invalidTitle)
    {
        // Arrange
        var command = new CreateMaintenanceRequestCommand(
            invalidTitle!,
            "Pipe burst in basement",
            MaintenancePriority.High,
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("العنوان مطلوب.");
    }

    [Fact]
    public void Validate_WithTitleExceeding200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longTitle = new string('T', 201);
        var command = new CreateMaintenanceRequestCommand(
            longTitle,
            "Pipe burst in basement",
            MaintenancePriority.High,
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("العنوان لا يمكن أن يتجاوز 200 حرف.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyDescription_ShouldHaveValidationError(string? invalidDescription)
    {
        // Arrange
        var command = new CreateMaintenanceRequestCommand(
            "Valid Title",
            invalidDescription!,
            MaintenancePriority.High,
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("الوصف مطلوب.");
    }

    [Fact]
    public void Validate_WithDescriptionExceeding2000Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longDesc = new string('D', 2001);
        var command = new CreateMaintenanceRequestCommand(
            "Valid Title",
            longDesc,
            MaintenancePriority.High,
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("الوصف لا يمكن أن يتجاوز 2000 حرف.");
    }

    [Fact]
    public void Validate_WithInvalidPriority_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateMaintenanceRequestCommand(
            "Valid Title",
            "Valid Description",
            (MaintenancePriority)999,
            Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Priority)
            .WithErrorMessage("أولوية الطلب غير صحيحة.");
    }

    [Fact]
    public void Validate_WithEmptyMaintenanceTypeId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateMaintenanceRequestCommand(
            "Valid Title",
            "Valid Description",
            MaintenancePriority.Medium,
            Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.MaintenanceTypeId)
            .WithErrorMessage("معرّف نوع الصيانة مطلوب.");
    }
}


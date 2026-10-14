using FluentAssertions;
using FluentValidation.TestHelper;
using MaintenanceSystem.Application.Features.Auth.Login;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var command = new LoginCommand("admin_user", "ValidPassword123!");

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
    public void Validate_WithEmptyUserName_ShouldHaveValidationError(string? invalidUserName)
    {
        // Arrange
        var command = new LoginCommand(invalidUserName!, "ValidPassword123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("اسم المستخدم مطلوب.");
    }

    [Fact]
    public void Validate_WithUserNameExceeding50Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longUserName = new string('u', 51);
        var command = new LoginCommand(longUserName, "ValidPassword123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("اسم المستخدم لا يجب أن يتجاوز 50 حرفاً.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyPassword_ShouldHaveValidationError(string? invalidPassword)
    {
        // Arrange
        var command = new LoginCommand("admin_user", invalidPassword!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("كلمة المرور مطلوبة.");
    }

    [Fact]
    public void Validate_WithPasswordExceeding128Characters_ShouldHaveValidationError()
    {
        // Arrange
        var longPassword = new string('p', 129);
        var command = new LoginCommand("admin_user", longPassword);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("كلمة المرور لا يجب أن تتجاوز 128 حرفاً.");
    }
}


using FluentAssertions;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Common.Exceptions;
using MaintenanceSystem.Application.Features.Auth.Login;
using MaintenanceSystem.Domain.Entities;
using Moq;
using Xunit;

namespace MaintenanceSystem.Tests.Application.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync("unknown_user", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("unknown_user", "Password123!");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("اسم المستخدم أو كلمة المرور غير صحيحة.");

        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsInactive_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User("inactive_user", "inactive@example.com", "hashed_pass");
        user.Deactivate();

        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand(user.UserName, "Password123!");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("اسم المستخدم أو كلمة المرور غير صحيحة.");

        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsLockedOut_ShouldThrowLockedException()
    {
        // Arrange
        var user = new User("locked_user", "locked@example.com", "hashed_pass");
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedAttempt();
        }

        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new LoginCommand(user.UserName, "Password123!");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<LockedException>()
            .WithMessage("*الحساب مقفول مؤقتاً*");

        _passwordHasherMock.Verify(
            h => h.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPasswordIsIncorrect_ShouldIncrementFailedAttemptsAndThrowUnauthorizedException()
    {
        // Arrange
        var user = new User("test_user", "test@example.com", "stored_hashed_pass");

        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify("WrongPassword", user.PasswordHash))
            .Returns(false);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new LoginCommand(user.UserName, "WrongPassword");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("اسم المستخدم أو كلمة المرور غير صحيحة.");

        user.FailedLoginCount.Should().Be(1);

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);

        _tokenServiceMock.Verify(
            t => t.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenFifthFailedAttemptOccurs_ShouldLockoutUser()
    {
        // Arrange
        var user = new User("test_user", "test@example.com", "stored_hashed_pass");
        for (var i = 0; i < 4; i++)
        {
            user.RegisterFailedAttempt();
        }

        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify("WrongPasswordAgain", user.PasswordHash))
            .Returns(false);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new LoginCommand(user.UserName, "WrongPasswordAgain");

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();

        user.FailedLoginCount.Should().Be(5);
        user.IsLockedOut().Should().BeTrue();

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ShouldResetLockoutAndReturnGeneratedToken()
    {
        // Arrange
        var user = new User("valid_user", "valid@example.com", "correct_hashed_pass");
        user.RegisterFailedAttempt(); // Prior failed attempt

        _userRepositoryMock
            .Setup(r => r.GetByUserNameAsync(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.Verify("CorrectPassword", user.PasswordHash))
            .Returns(true);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var expectedExpiry = DateTime.UtcNow.AddHours(1);
        _tokenServiceMock
            .Setup(t => t.GenerateToken(user))
            .Returns(("generated-jwt-token-xyz", expectedExpiry));

        var command = new LoginCommand(user.UserName, "CorrectPassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("generated-jwt-token-xyz");
        result.ExpiresAtUtc.Should().Be(expectedExpiry);

        user.FailedLoginCount.Should().Be(0);
        user.LockoutEndUtc.Should().BeNull();

        _userRepositoryMock.Verify(
            r => r.UpdateAsync(user, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}


using FluentAssertions;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Exceptions;
using Xunit;

namespace MaintenanceSystem.Tests.Domain;

public class UserTests
{
    private static User CreateValidUser(
        string userName = "john_doe",
        string email = "john.doe@example.com",
        string passwordHash = "$2a$12$e80yvV8gWnJ14wX.K8hGqum5tF8yWnJ14wX.K8hGqum5tF8yWnJ14")
    {
        return new User(userName, email, passwordHash);
    }

    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeCorrectly()
    {
        // Act
        var user = new User("  alice  ", " Alice@Example.COM ", "some_valid_hash");

        // Assert
        user.UserName.Should().Be("alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("some_valid_hash");
        user.IsActive.Should().BeTrue();
        user.FailedLoginCount.Should().Be(0);
        user.LockoutEndUtc.Should().BeNull();
        user.SecurityStamp.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyUserName_ShouldThrowBusinessRuleViolationException(string? invalidUserName)
    {
        // Act
        Action act = () => new User(invalidUserName!, "test@example.com", "hash");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*UserName cannot be empty*");
    }

    [Fact]
    public void Constructor_WithUserNameExceeding50Characters_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var longUserName = new string('u', 51);

        // Act
        Action act = () => new User(longUserName, "test@example.com", "hash");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*UserName cannot exceed 50 characters*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("plainaddress")]
    [InlineData("@missinguser.com")]
    [InlineData("missingdomain@")]
    public void Constructor_WithInvalidEmail_ShouldThrowBusinessRuleViolationException(string? invalidEmail)
    {
        // Act
        Action act = () => new User("valid_user", invalidEmail!, "hash");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyPasswordHash_ShouldThrowBusinessRuleViolationException(string? invalidHash)
    {
        // Act
        Action act = () => new User("valid_user", "test@example.com", invalidHash!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*PasswordHash cannot be empty*");
    }

    [Fact]
    public void RegisterFailedAttempt_FourTimes_ShouldNotLockoutUser()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        for (var i = 0; i < 4; i++)
        {
            user.RegisterFailedAttempt(now.AddMinutes(i));
        }

        // Assert
        user.FailedLoginCount.Should().Be(4);
        user.LockoutEndUtc.Should().BeNull();
        user.IsLockedOut(now.AddMinutes(4)).Should().BeFalse();
    }

    [Fact]
    public void RegisterFailedAttempt_FifthTime_ShouldLockoutUserFor15Minutes()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedAttempt(now.AddMinutes(i));
        }

        // Assert
        user.FailedLoginCount.Should().Be(5);
        user.LockoutEndUtc.Should().Be(now.AddMinutes(4).Add(User.LockoutDuration));
        user.IsLockedOut(now.AddMinutes(4)).Should().BeTrue();
        user.IsLockedOut(now.AddMinutes(4).AddMinutes(14)).Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_AfterLockoutDurationExpires_ShouldReturnFalse()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedAttempt(now);
        }

        // Act
        var isLockedAfter16Minutes = user.IsLockedOut(now.AddMinutes(16));

        // Assert
        isLockedAfter16Minutes.Should().BeFalse();
    }

    [Fact]
    public void ResetLockout_WhenUserIsLockedOut_ShouldResetCountAndClearLockoutDate()
    {
        // Arrange
        var user = CreateValidUser();
        var now = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedAttempt(now);
        }

        user.IsLockedOut(now).Should().BeTrue();

        // Act
        var resetTime = now.AddMinutes(2);
        user.ResetLockout(resetTime);

        // Assert
        user.FailedLoginCount.Should().Be(0);
        user.LockoutEndUtc.Should().BeNull();
        user.IsLockedOut(resetTime).Should().BeFalse();
        user.UpdatedAtUtc.Should().Be(resetTime);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveFalseAndRegenerateSecurityStamp()
    {
        // Arrange
        var user = CreateValidUser();
        var initialStamp = user.SecurityStamp;
        var now = new DateTime(2026, 10, 15, 13, 0, 0, DateTimeKind.Utc);

        // Act
        user.Deactivate(now);

        // Assert
        user.IsActive.Should().BeFalse();
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void Activate_WhenInactive_ShouldSetIsActiveTrue()
    {
        // Arrange
        var user = CreateValidUser();
        user.Deactivate();

        var now = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc);

        // Act
        user.Activate(now);

        // Assert
        user.IsActive.Should().BeTrue();
        user.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdateHashAndRegenerateSecurityStamp()
    {
        // Arrange
        var user = CreateValidUser();
        var initialStamp = user.SecurityStamp;
        const string newHash = "new_secure_bcrypt_hash_value_1234567890";
        var now = new DateTime(2026, 10, 15, 15, 0, 0, DateTimeKind.Utc);

        // Act
        user.UpdatePassword(newHash, now);

        // Assert
        user.PasswordHash.Should().Be(newHash);
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void AssignRole_WithValidRoleId_ShouldAddUserRoleAndRegenerateSecurityStamp()
    {
        // Arrange
        var user = CreateValidUser();
        var initialStamp = user.SecurityStamp;
        var roleId = Guid.NewGuid();
        var now = new DateTime(2026, 10, 15, 16, 0, 0, DateTimeKind.Utc);

        // Act
        user.AssignRole(roleId, now);

        // Assert
        user.UserRoles.Should().ContainSingle(r => r.RoleId == roleId);
        user.SecurityStamp.Should().NotBe(initialStamp);
        user.UpdatedAtUtc.Should().Be(now);
    }

    [Fact]
    public void AssignRole_WithEmptyGuid_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var user = CreateValidUser();

        // Act
        Action act = () => user.AssignRole(Guid.Empty);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("*RoleId cannot be empty*");
    }

    [Fact]
    public void AssignRole_WhenAlreadyAssigned_ShouldNotAddDuplicate()
    {
        // Arrange
        var user = CreateValidUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);

        // Act
        user.AssignRole(roleId);

        // Assert
        user.UserRoles.Should().ContainSingle(r => r.RoleId == roleId);
    }

    [Fact]
    public void RemoveRole_WhenExists_ShouldRemoveRoleAndRegenerateSecurityStamp()
    {
        // Arrange
        var user = CreateValidUser();
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId);
        var stampAfterAssign = user.SecurityStamp;
        var removeTime = new DateTime(2026, 10, 15, 17, 0, 0, DateTimeKind.Utc);

        // Act
        user.RemoveRole(roleId, removeTime);

        // Assert
        user.UserRoles.Should().BeEmpty();
        user.SecurityStamp.Should().NotBe(stampAfterAssign);
        user.UpdatedAtUtc.Should().Be(removeTime);
    }
}

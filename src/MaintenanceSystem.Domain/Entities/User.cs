using System.Net.Mail;
using MaintenanceSystem.Domain.Common;
using MaintenanceSystem.Domain.Exceptions;

namespace MaintenanceSystem.Domain.Entities;

public class User : Entity
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string UserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public Guid SecurityStamp { get; private set; }
    public bool IsActive { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { } // Required for EF Core

    public User(string userName, string email, string passwordHash)
    {
        ValidateUserName(userName);
        ValidateEmail(email);
        ValidatePasswordHash(passwordHash);

        UserName = userName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        SecurityStamp = Guid.CreateVersion7();
        IsActive = true;
        FailedLoginCount = 0;
        LockoutEndUtc = null;
    }

    public void RegisterFailedAttempt(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        FailedLoginCount++;

        if (FailedLoginCount >= MaxFailedAttempts)
        {
            LockoutEndUtc = now.Add(LockoutDuration);
        }

        UpdatedAtUtc = now;
    }

    public bool IsLockedOut(DateTime? utcNow = null)
    {
        var now = utcNow ?? DateTime.UtcNow;
        return LockoutEndUtc.HasValue && LockoutEndUtc.Value > now;
    }

    public void ResetLockout(DateTime? utcNow = null)
    {
        if (FailedLoginCount > 0 || LockoutEndUtc != null)
        {
            FailedLoginCount = 0;
            LockoutEndUtc = null;
            UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
        }
    }

    public void RegenerateSecurityStamp(DateTime? utcNow = null)
    {
        SecurityStamp = Guid.CreateVersion7();
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash, DateTime? utcNow = null)
    {
        ValidatePasswordHash(newPasswordHash);

        PasswordHash = newPasswordHash;
        RegenerateSecurityStamp(utcNow);
    }

    public void Deactivate(DateTime? utcNow = null)
    {
        if (!IsActive) return;

        IsActive = false;
        RegenerateSecurityStamp(utcNow);
    }

    public void Activate(DateTime? utcNow = null)
    {
        if (IsActive) return;

        IsActive = true;
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    public void AssignRole(Guid roleId, DateTime? utcNow = null)
    {
        if (roleId == Guid.Empty)
            throw new BusinessRuleViolationException("RoleId cannot be empty.");

        if (_userRoles.Any(r => r.RoleId == roleId))
            return;

        _userRoles.Add(new UserRole(Id, roleId));
        RegenerateSecurityStamp(utcNow);
    }

    public void RemoveRole(Guid roleId, DateTime? utcNow = null)
    {
        var existing = _userRoles.FirstOrDefault(r => r.RoleId == roleId);
        if (existing == null)
            return;

        _userRoles.Remove(existing);
        RegenerateSecurityStamp(utcNow);
    }

    private static void ValidateUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new BusinessRuleViolationException("UserName cannot be empty.");

        if (userName.Trim().Length > 50)
            throw new BusinessRuleViolationException("UserName cannot exceed 50 characters.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleViolationException("Email cannot be empty.");

        var trimmed = email.Trim();
        if (trimmed.Length > 256)
            throw new BusinessRuleViolationException("Email cannot exceed 256 characters.");

        if (!trimmed.Contains('@') || !MailAddress.TryCreate(trimmed, out var address) || address.Address != trimmed)
            throw new BusinessRuleViolationException("Invalid email format.");
    }

    private static void ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessRuleViolationException("PasswordHash cannot be empty.");

        if (passwordHash.Length > 200)
            throw new BusinessRuleViolationException("PasswordHash cannot exceed 200 characters.");
    }
}


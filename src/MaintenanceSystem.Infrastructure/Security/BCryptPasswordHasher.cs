using MaintenanceSystem.Application.Abstractions.Security;

namespace MaintenanceSystem.Infrastructure.Security;

/// <summary>
/// BCrypt implementation of <see cref="IPasswordHasher"/> using BCrypt.Net-Next.
/// Fixed Work Factor = 11 as specified in ADR-0001 (Section 3.2 and 4.5).
/// BCrypt automatically embeds the salt inside the resulting hash string; no separate salt column is needed.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public const int WorkFactor = 11;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}


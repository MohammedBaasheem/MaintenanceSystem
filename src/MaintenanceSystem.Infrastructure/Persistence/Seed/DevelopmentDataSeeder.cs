using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MaintenanceSystem.Infrastructure.Persistence.Seed;

public class DevelopmentDataSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            _logger.LogWarning("DevelopmentDataSeeder skipped: current environment is '{EnvironmentName}'. Seeder runs only in Development.", _environment.EnvironmentName);
            return;
        }

        _logger.LogInformation("Starting DevelopmentDataSeeder...");

        // 1. Seed Roles (Idempotent)
        var requiredRoles = new (string Name, string Description)[]
        {
            ("MaintenanceAdmin", "Maintenance Administrator"),
            ("Employee", "Regular Employee"),
            ("Technician", "Maintenance Technician")
        };

        var roleMap = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, description) in requiredRoles)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
            if (existingRole == null)
            {
                existingRole = new Role(name, description);
                await _context.Roles.AddAsync(existingRole, cancellationToken);
                _logger.LogInformation("Seeded role: '{RoleName}'.", name);
            }
            roleMap[name] = existingRole;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // 2. Seed Users (Idempotent)
        var seedUsers = new (string UserName, string Email, string SecretKey, string RoleName)[]
        {
            ("admin", "admin@system.local", "Seed:AdminPassword", "MaintenanceAdmin"),
            ("employee", "employee@system.local", "Seed:EmployeePassword", "Employee"),
            ("technician", "technician@system.local", "Seed:TechnicianPassword", "Technician")
        };

        foreach (var (userName, email, secretKey, roleName) in seedUsers)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserName == userName, cancellationToken);
            if (userExists)
            {
                _logger.LogInformation("User '{UserName}' already exists. Skipping.", userName);
                continue;
            }

            var password = _configuration[secretKey];
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException($"Password secret '{secretKey}' is not configured in User Secrets for user '{userName}'.");
            }

            var passwordHash = _passwordHasher.Hash(password);
            var user = new User(userName, email, passwordHash);

            if (roleMap.TryGetValue(roleName, out var role))
            {
                user.AssignRole(role.Id);
            }
            else
            {
                throw new InvalidOperationException($"Role '{roleName}' was not found when seeding user '{userName}'.");
            }

            await _context.Users.AddAsync(user, cancellationToken);
            _logger.LogInformation("Seeded user: '{UserName}' with role '{RoleName}'.", userName, roleName);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("DevelopmentDataSeeder completed successfully.");
    }
}


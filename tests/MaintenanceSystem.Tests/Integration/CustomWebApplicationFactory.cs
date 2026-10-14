using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MaintenanceSystem.API.Contracts.Auth;
using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Infrastructure.Persistence;
using Role = MaintenanceSystem.Domain.Entities.Role;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MaintenanceSystem.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;
    private readonly object _lock = new();
    private bool _isSeeded;

    public Guid SeededMaintenanceTypeId { get; private set; }
    public Guid TechnicianUserId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", "DataSource=:memory:");
        builder.UseSetting("ConnectionStrings:Redis", "localhost:6379");
        builder.UseSetting("Jwt:SigningKey", "SuperSecretIntegrationTestingSigningKey1234567890!XYZ");
        builder.UseSetting("Jwt:Issuer", "MaintenanceSystemAPI");
        builder.UseSetting("Jwt:Audience", "MaintenanceSystemClients");
        builder.UseSetting("Jwt:LifetimeMinutes", "60");

        builder.ConfigureServices(services =>
        {
            var efDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true ||
                d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true ||
                d.ServiceType.FullName?.Contains("SqlServer") == true ||
                d.ImplementationType?.FullName?.Contains("SqlServer") == true
            ).ToList();

            foreach (var d in efDescriptors)
            {
                services.Remove(d);
            }

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _connection.CreateFunction("LEN", (string? s) => s == null ? 0 : s.TrimEnd().Length);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            var redisMultiplexerDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisMultiplexerDescriptor != null)
            {
                services.Remove(redisMultiplexerDescriptor);
            }

            var cacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMaintenanceTypeCache));
            if (cacheDescriptor != null)
            {
                services.Remove(cacheDescriptor);
            }
            services.AddSingleton<IMaintenanceTypeCache, InMemoryTestMaintenanceTypeCache>();
        });
    }

    public void EnsureDatabaseSeeded()
    {
        lock (_lock)
        {
            if (_isSeeded) return;

            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            context.Database.EnsureCreated();

            var adminRole = new Role("MaintenanceAdmin", "Maintenance Administrator");
            var employeeRole = new Role("Employee", "Regular Employee");
            var techRole = new Role("Technician", "Maintenance Technician");

            context.Roles.AddRange(adminRole, employeeRole, techRole);
            context.SaveChanges();

            var adminUser = new User("admin_test", "admin@test.local", hasher.Hash("Admin@123456"));
            adminUser.AssignRole(adminRole.Id);

            var employeeUser = new User("employee_test", "employee@test.local", hasher.Hash("Employee@123456"));
            employeeUser.AssignRole(employeeRole.Id);

            var techUser = new User("tech_test", "tech@test.local", hasher.Hash("Tech@123456"));
            techUser.AssignRole(techRole.Id);

            context.Users.AddRange(adminUser, employeeUser, techUser);

            var maintenanceType = new MaintenanceType("HVAC Repair", "Cooling and heating systems");
            context.MaintenanceTypes.Add(maintenanceType);

            context.SaveChanges();

            SeededMaintenanceTypeId = maintenanceType.Id;
            TechnicianUserId = techUser.Id;
            _isSeeded = true;
        }
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string userName, string password)
    {
        EnsureDatabaseSeeded();
        var client = CreateClient();

        var loginRequest = new LoginRequest(userName, password);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Dispose();
        }
    }

    private sealed class InMemoryTestMaintenanceTypeCache : IMaintenanceTypeCache
    {
        private IReadOnlyList<MaintenanceTypeDto>? _cache;

        public Task<IReadOnlyList<MaintenanceTypeDto>?> GetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_cache);

        public Task SetAsync(IReadOnlyList<MaintenanceTypeDto> types, CancellationToken cancellationToken = default)
        {
            _cache = types;
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            _cache = null;
            return Task.CompletedTask;
        }
    }
}

using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Infrastructure.Persistence;
using MaintenanceSystem.Infrastructure.Persistence.Repositories;
using MaintenanceSystem.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MaintenanceSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        „Ê«›ﬁ ⁄·Ï «·«Œ »«—.

         –ﬂÌ—:
-«” Œœ„.ps1 Ê«Õœ ›Ì C:\Temp\.
-·« polling „ﬂ——.
-·«  ﬁ—√ transcript.jsonl.
- √Êﬁ› «· ÿ»Ìﬁ Ê‰Ÿ¯› DB »⁄œ «·«‰ Â«¡.
- «⁄—÷  ﬁ—Ì—« „ÊÃ“«(Status Code ·ﬂ· ”Ì‰«—ÌÊ).        services.AddScoped<IRoleRepository, RoleRepository>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection(JwtOptions.SectionName);
            if (section.Exists())
            {
                options.Issuer = section["Issuer"] ?? options.Issuer;
                options.Audience = section["Audience"] ?? options.Audience;
                if (int.TryParse(section["LifetimeMinutes"], out var lifetime))
                {
                    options.LifetimeMinutes = lifetime;
                }
            }

            var signingKey = configuration["Jwt:SigningKey"];
            if (!string.IsNullOrWhiteSpace(signingKey))
            {
                options.SigningKey = signingKey;
            }
        });

        services.AddScoped<ITokenService, JwtTokenService>();

        return services;
    }
}


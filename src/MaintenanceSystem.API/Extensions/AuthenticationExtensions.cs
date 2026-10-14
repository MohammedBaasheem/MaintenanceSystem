using System.Security.Claims;
using System.Text;
using MaintenanceSystem.Application.Abstractions.Persistence;
using MaintenanceSystem.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MaintenanceSystem.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = configuration["Jwt:SigningKey"];
        if (!string.IsNullOrWhiteSpace(signingKey))
        {
            jwtOptions.SigningKey = signingKey;
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = "role",
                NameClaimType = "unique_name"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var principal = context.Principal;
                    if (principal == null)
                    {
                        context.Fail("Unauthorized: No principal found in token.");
                        return;
                    }

                    // a. sub claim exists?
                    var subClaim = principal.FindFirst("sub")?.Value
                        ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (string.IsNullOrWhiteSpace(subClaim) || !Guid.TryParse(subClaim, out var userId))
                    {
                        context.Fail("Unauthorized: Invalid or missing 'sub' claim.");
                        return;
                    }

                    // b. GetByIdAsync(userId) — does user exist?
                    var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                    var user = await userRepository.GetByIdAsync(userId, context.HttpContext.RequestAborted);

                    if (user == null)
                    {
                        context.Fail("Unauthorized: User not found.");
                        return;
                    }

                    // c. user.IsActive == true?
                    if (!user.IsActive)
                    {
                        context.Fail("Unauthorized: User account is inactive.");
                        return;
                    }

                    // d. user.SecurityStamp.ToString() == claim security_stamp?
                    var securityStampClaim = principal.FindFirst("security_stamp")?.Value;
                    if (string.IsNullOrWhiteSpace(securityStampClaim) ||
                        !string.Equals(user.SecurityStamp.ToString(), securityStampClaim, StringComparison.OrdinalIgnoreCase))
                    {
                        context.Fail("Unauthorized: Security stamp has changed or is invalid. Token is revoked.");
                        return;
                    }
                }
            };
        });

        return services;
    }
}


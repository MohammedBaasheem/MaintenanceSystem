using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MaintenanceSystem.Application.Abstractions.Security;
using MaintenanceSystem.Domain.Entities;
using MaintenanceSystem.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MaintenanceSystem.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.OutboundClaimTypeMap.Clear();

        var key = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_jwtOptions.LifetimeMinutes);

        var primaryRole = user.UserRoles.FirstOrDefault()?.Role?.Name ?? RoleNames.Employee;

        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("nameid", user.Id.ToString()),
            new("unique_name", user.UserName),
            new("email", user.Email),
            new("role", primaryRole),
            new("security_stamp", user.SecurityStamp.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAtUtc,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            IssuedAt = now,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiresAtUtc);
    }
}


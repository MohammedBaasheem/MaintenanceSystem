using MaintenanceSystem.Domain.Enums;

namespace MaintenanceSystem.API.Authorization;

public static class PolicyExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.MaintenanceAdminOnly, policy =>
                policy.RequireRole(RoleNames.MaintenanceAdmin));

            options.AddPolicy(Policies.TechnicianOrAdmin, policy =>
                policy.RequireRole(RoleNames.MaintenanceAdmin, RoleNames.Technician));

            options.AddPolicy(Policies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}


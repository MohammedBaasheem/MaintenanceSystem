namespace MaintenanceSystem.API.Authorization;

public static class Policies
{
    public const string MaintenanceAdminOnly = nameof(MaintenanceAdminOnly);
    public const string TechnicianOrAdmin = nameof(TechnicianOrAdmin);
    public const string AuthenticatedUser = nameof(AuthenticatedUser);
}


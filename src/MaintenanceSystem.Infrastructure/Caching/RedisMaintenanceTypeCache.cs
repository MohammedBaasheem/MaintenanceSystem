using System.Text.Json;
using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MaintenanceSystem.Infrastructure.Caching;

public class RedisMaintenanceTypeCache : IMaintenanceTypeCache
{
    private const string CacheKey = "maintenance-types:active:v1";
    private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisMaintenanceTypeCache> _logger;

    public RedisMaintenanceTypeCache(
        IConnectionMultiplexer redis,
        ILogger<RedisMaintenanceTypeCache> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MaintenanceTypeDto>?> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(CacheKey);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<IReadOnlyList<MaintenanceTypeDto>>((string)value!, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve cache for key {CacheKey}. Falling back to database.", CacheKey);
            return null;
        }
    }

    public async Task SetAsync(IReadOnlyList<MaintenanceTypeDto> types, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(types, JsonOptions);

            await db.StringSetAsync(CacheKey, json, Expiry);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache for key {CacheKey}.", CacheKey);
        }
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(CacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for key {CacheKey}.", CacheKey);
        }
    }
}


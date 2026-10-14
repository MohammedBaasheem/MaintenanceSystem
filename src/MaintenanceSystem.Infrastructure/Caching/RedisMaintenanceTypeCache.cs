using System.Text.Json;
using MaintenanceSystem.Application.Abstractions.Caching;
using MaintenanceSystem.Application.Features.MaintenanceTypes.GetActiveMaintenanceTypes;
using StackExchange.Redis;

namespace MaintenanceSystem.Infrastructure.Caching;

public class RedisMaintenanceTypeCache : IMaintenanceTypeCache
{
    private const string CacheKey = "maintenance-types:active:v1";
    private static readonly TimeSpan Expiry = TimeSpan.FromMinutes(10);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IConnectionMultiplexer _redis;

    public RedisMaintenanceTypeCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<IReadOnlyList<MaintenanceTypeDto>?> GetAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(CacheKey);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<IReadOnlyList<MaintenanceTypeDto>>((string)value!, JsonOptions);
    }

    public async Task SetAsync(IReadOnlyList<MaintenanceTypeDto> types, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(types, JsonOptions);

        await db.StringSetAsync(CacheKey, json, Expiry);
    }

    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CacheKey);
    }
}

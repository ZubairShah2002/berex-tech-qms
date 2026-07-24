using System.Text.Json;
using BerexQms.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace BerexQms.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value!, SerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cache value for key {Key}", key);
            await db.KeyDeleteAsync(key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var serialized = JsonSerializer.Serialize(value, SerializerOptions);
        await db.StringSetAsync(key, serialized, expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}

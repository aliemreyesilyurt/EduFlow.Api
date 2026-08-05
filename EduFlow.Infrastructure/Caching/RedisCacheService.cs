using System.Text.Json;
using EduFlow.Application.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace EduFlow.Infrastructure.Caching;

public sealed class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer multiplexer) : ICacheService
{
    // Bounds how long a lock survives if its holder crashes before releasing it, and how long a
    // waiter will poll before giving up on the holder and computing the value itself.
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MaxWait = TimeSpan.FromSeconds(12);

    // Spreads out expirations of entries set around the same time (e.g. a warm-up or a burst of
    // first-time misses) so they don't all expire in the same instant and reproduce the stampede
    // the lock above already guards against.
    private const double JitterRatio = 0.10;
    private static readonly TimeSpan MaxJitter = TimeSpan.FromSeconds(30);

    private const string ReleaseLockScript = """
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        end
        return 0
        """;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        var baseExpiration = expiration ?? TimeSpan.FromMinutes(10);
        var jitterCapMs = Math.Min(baseExpiration.TotalMilliseconds * JitterRatio, MaxJitter.TotalMilliseconds);
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, (int)jitterCapMs + 1));

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = baseExpiration + jitter
        };

        await cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(key, cancellationToken);

    // Protects against cache stampede: when a key is missing, only the caller (across all instances)
    // that wins the Redis lock runs the factory; everyone else polls the cache instead of re-running it.
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var db = multiplexer.GetDatabase();
        var lockKey = $"lock:{key}";
        var lockToken = Guid.NewGuid().ToString("N");
        var deadline = DateTime.UtcNow + MaxWait;

        while (DateTime.UtcNow < deadline)
        {
            if (await db.StringSetAsync(lockKey, lockToken, LockTtl, When.NotExists))
            {
                try
                {
                    // Another instance may have populated the cache between our first read and
                    // winning the lock.
                    cached = await GetAsync<T>(key, cancellationToken);
                    if (cached is not null)
                    {
                        return cached;
                    }

                    var value = await factory(cancellationToken);
                    await SetAsync(key, value, expiration, cancellationToken);
                    return value;
                }
                finally
                {
                    await db.ScriptEvaluateAsync(ReleaseLockScript, [lockKey], [lockToken]);
                }
            }

            await Task.Delay(Random.Shared.Next(50, 150), cancellationToken);
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached is not null)
            {
                return cached;
            }
        }

        // The lock holder appears to have died without releasing it or writing the value; compute
        // directly rather than block the caller any longer.
        var fallbackValue = await factory(cancellationToken);
        await SetAsync(key, fallbackValue, expiration, cancellationToken);
        return fallbackValue;
    }
}

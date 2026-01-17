using Domain.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;
namespace Infrastructure.Aggregator.Caching;
public sealed class AsyncCache<TValue> : IAsyncCache<TValue>, IDisposable
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly AsyncKeyedLock<string> _keyedLock = new();
    private readonly Channel<string> _evictionChannel;
    private readonly ILogger<AsyncCache<TValue>> _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _evictionTask;
    private bool _disposed;

    public AsyncCache(ILogger<AsyncCache<TValue>> logger)
    {
        _logger = logger;
        _evictionChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _evictionTask = ProcessEvictionsAsync(_cts.Token);
    }

    public Task<TValue?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_cache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                Interlocked.Increment(ref entry.HitCount);
                return Task.FromResult<TValue?>(entry.Value);
            }
            _evictionChannel.Writer.TryWrite(key);
        }

        return Task.FromResult<TValue?>(default);
    }

    public Task SetAsync(string key, TValue value, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var entry = new CacheEntry(value, duration);
        _cache.AddOrUpdate(key, entry, (_, _) => entry);

        _logger.LogDebug("Cache SET: {Key}, expires in {Duration}s", key, duration.TotalSeconds);
        return Task.CompletedTask;
    }

    public async Task<TValue> GetOrCreateAsync(
        string key,
        Func<CancellationToken, Task<TValue>> factory,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_cache.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            Interlocked.Increment(ref entry.HitCount);
            _logger.LogDebug("Cache HIT: {Key}", key);
            return entry.Value;
        }

        using (await _keyedLock.LockAsync(key, cancellationToken).ConfigureAwait(false))
        {
            if (_cache.TryGetValue(key, out entry) && !entry.IsExpired)
            {
                Interlocked.Increment(ref entry.HitCount);
                return entry.Value;
            }

            _logger.LogDebug("Cache MISS: {Key}, invoking factory", key);

            var value = await factory(cancellationToken).ConfigureAwait(false);
            var newEntry = new CacheEntry(value, duration);
            _cache.AddOrUpdate(key, newEntry, (_, _) => newEntry);

            return value;
        }
    }

    public void Invalidate(string key)
    {
        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug("Cache INVALIDATE: {Key}", key);
        }
    }
    public void InvalidateAll()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation("Cache CLEAR: {Count} entries removed", count);
    }
    private async Task ProcessEvictionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var key in _evictionChannel.Reader.ReadAllAsync(cancellationToken))
            {
                if (_cache.TryGetValue(key, out var entry) && entry.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    _logger.LogDebug("Cache EVICT: {Key}", key);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _evictionChannel.Writer.Complete();
        _cts.Cancel();

        try
        {
            _evictionTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch { /* Ignore shutdown errors */ }

        _cts.Dispose();
        _keyedLock.Dispose();
        _cache.Clear();

        _logger.LogInformation("AsyncCache disposed");
    }

    private sealed class CacheEntry
    {
        public TValue Value { get; }
        public DateTime ExpiresAt { get; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
        public int HitCount;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsStale(TimeSpan staleThreshold) => DateTime.UtcNow >= ExpiresAt - staleThreshold;

        public CacheEntry(TValue value, TimeSpan duration)
        {
            Value = value;
            ExpiresAt = DateTime.UtcNow.Add(duration);
        }
    }
}
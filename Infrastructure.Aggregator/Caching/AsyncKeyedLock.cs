using System.Collections.Concurrent;

namespace Infrastructure.Aggregator.Caching;

public sealed class AsyncKeyedLock<TKey> : IDisposable where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, LockEntry> _locks = new();
    private readonly TimeSpan _cleanupInterval;
    private readonly Timer _cleanupTimer;
    private bool _disposed;
    public AsyncKeyedLock(TimeSpan? cleanupInterval = null)
    {
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(5);
        _cleanupTimer = new Timer(CleanupUnusedLocks, null, _cleanupInterval, _cleanupInterval);
    }
    public async Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var entry = _locks.GetOrAdd(key, _ => new LockEntry());
        Interlocked.Increment(ref entry.ReferenceCount);
        await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        return new LockReleaser(this, key, entry);
    }

    private void Release(TKey key, LockEntry entry)
    {
        entry.Semaphore.Release();
        entry.LastAccess = DateTime.UtcNow;

        if (Interlocked.Decrement(ref entry.ReferenceCount) == 0)
        {
            entry.LastAccess = DateTime.UtcNow;
        }
    }

    private void CleanupUnusedLocks(object? state)
    {
        if (_disposed) return;

        var threshold = DateTime.UtcNow - _cleanupInterval;
        foreach (var kvp in _locks)
        {
            if (kvp.Value.ReferenceCount == 0 && kvp.Value.LastAccess < threshold)
            {
                if (_locks.TryRemove(kvp.Key, out var removed))
                {
                    removed.Semaphore.Dispose();
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cleanupTimer.Dispose();
        foreach (var entry in _locks.Values)
        {
            entry.Semaphore.Dispose();
        }
        _locks.Clear();
    }

    private sealed class LockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount;
        public DateTime LastAccess = DateTime.UtcNow;
    }

    private sealed class LockReleaser(AsyncKeyedLock<TKey> parent, TKey key, LockEntry entry) : IDisposable
    {
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            parent.Release(key, entry);
        }
    }
}
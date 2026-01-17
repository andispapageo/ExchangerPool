namespace Domain.Core.Interfaces;
public interface IAsyncCache<TValue>
{
    Task<TValue?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, TValue value, TimeSpan duration, CancellationToken cancellationToken = default);
    Task<TValue> GetOrCreateAsync(string key, Func<CancellationToken, Task<TValue>> factory, TimeSpan duration, CancellationToken cancellationToken = default);
    void Invalidate(string key);
    void InvalidateAll();
}
using Domain.Core.Entities.Aggregates;
using Domain.Core.Entities.Entities;
using Domain.Core.Exceptions;
using Domain.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Infrastructure.Aggregator;

public sealed class LiquidityAggregator : ILiquidityAggregator, IDisposable
{
    private readonly IReadOnlyList<IExchangeClient> _exchangeClients;
    private readonly IAsyncCache<AggregatedPrice> _priceCache;
    private readonly IAsyncCache<IReadOnlyList<CryptoSymbol>> _symbolCache;
    private readonly ILogger<LiquidityAggregator> _logger;
    private readonly SemaphoreSlim _allPricesThrottle;
    private bool _disposed;

    private const string SymbolsCacheKey = "all_symbols";
    private static readonly TimeSpan PriceCacheDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan SymbolCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private const int MaxParallelExchangeCalls = 10;
    private const int MinExchangesRequired = 1;

    public LiquidityAggregator(
        IEnumerable<IExchangeClient> exchangeClients,
        IAsyncCache<AggregatedPrice> priceCache,
        IAsyncCache<IReadOnlyList<CryptoSymbol>> symbolCache,
        ILogger<LiquidityAggregator> logger)
    {
        ArgumentNullException.ThrowIfNull(exchangeClients);
        ArgumentNullException.ThrowIfNull(priceCache);
        ArgumentNullException.ThrowIfNull(symbolCache);
        ArgumentNullException.ThrowIfNull(logger);

        _exchangeClients = exchangeClients.ToList().AsReadOnly();
        _priceCache = priceCache;
        _symbolCache = symbolCache;
        _logger = logger;
        _allPricesThrottle = new SemaphoreSlim(MaxParallelExchangeCalls, MaxParallelExchangeCalls);

        if (_exchangeClients.Count == 0)
        {
            throw new ArgumentException("At least one exchange client is required.", nameof(exchangeClients));
        }

        _logger.LogInformation(
            "LiquidityAggregator initialized with {Count} exchange clients: {Exchanges}",
            _exchangeClients.Count,
            string.Join(", ", _exchangeClients.Select(c => c.ExchangeName)));
    }

    public async Task<AggregatedPrice> GetBestPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ValidateSymbol(symbol);

        var cacheKey = $"price_{symbol.ToUpperInvariant()}";

        return await _priceCache.GetOrCreateAsync(
            cacheKey,
            async ct => await FetchAggregatedPriceAsync(symbol, ct).ConfigureAwait(false),
            PriceCacheDuration,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<AggregatedPrice>> GetAllBestPricesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var allPrices = await FetchAllPricesWithChannelAsync(cancellationToken).ConfigureAwait(false);

        if (allPrices.Count == 0)
        {
            _logger.LogWarning("No prices retrieved from any exchange");
            return [];
        }

        var aggregated = allPrices
            .GroupBy(p => p.Symbol)
            .Where(g => g.Count() > 1)
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .WithCancellation(cancellationToken)
            .Select(g => AggregatedPrice.Create(g.Key, g))
            .ToList();

        return aggregated;
    }

    public async Task<IEnumerable<CryptoSymbol>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var symbols = await _symbolCache.GetOrCreateAsync(
            SymbolsCacheKey,
            FetchAllSymbolsAsync,
            SymbolCacheDuration,
            cancellationToken).ConfigureAwait(false);

        return symbols;
    }

    public async Task<IEnumerable<AggregatedPrice>> GetArbitrageRiskAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var allPrices = await GetAllBestPricesAsync(cancellationToken).ConfigureAwait(false);

        return allPrices
            .Where(p => p.HasArbitrageOpportunity)
            .OrderByDescending(p => p.ArbitrageRisk)
            .ToList();
    }

    private async Task<AggregatedPrice> FetchAggregatedPriceAsync(string symbol, CancellationToken cancellationToken)
    {
        var results = await FetchPricesFromAllExchangesAsync(symbol, cancellationToken).ConfigureAwait(false);

        var successResults = results.Where(r => r.IsSuccess && r.Data is not null).ToList();
        var failedResults = results.Where(r => !r.IsSuccess).ToList();

        // Log failures with details
        foreach (var failure in failedResults)
        {
            LogExchangeFailure(failure, symbol);
        }

        if (successResults.Count == 0)
        {
            var exchangeNames = _exchangeClients.Select(c => c.ExchangeName);
            throw new NoPriceDataException(symbol, exchangeNames);
        }

        // Warn about partial failures
        if (failedResults.Count > 0 && successResults.Count >= MinExchangesRequired)
        {
            _logger.LogWarning(
                "Partial price data for {Symbol}: {SuccessCount} succeeded, {FailCount} failed. Failed: {FailedExchanges}",
                symbol,
                successResults.Count,
                failedResults.Count,
                string.Join(", ", failedResults.Select(f => f.ExchangeName)));
        }

        var validPrices = successResults.Select(r => r.Data!).ToList();
        _logger.LogDebug("Aggregated {Count} prices for {Symbol}", validPrices.Count, symbol);

        return AggregatedPrice.Create(symbol, validPrices);
    }

    private async Task<List<ExchangeCallResult<ExchangePrice>>> FetchPricesFromAllExchangesAsync(
        string symbol,
        CancellationToken cancellationToken)
    {
        var tasks = _exchangeClients.Select(client =>
            ExecuteWithResultAsync(
                client,
                () => client.GetPriceAsync(symbol, cancellationToken),
                cancellationToken));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return [.. results];
    }

    private async Task<List<ExchangePrice>> FetchAllPricesWithChannelAsync(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<ExchangePrice>(new UnboundedChannelOptions
        {
            SingleWriter = false,
            SingleReader = true
        });

        var errors = new System.Collections.Concurrent.ConcurrentDictionary<string, Exception>();

        var producerTasks = _exchangeClients.Select(async client =>
        {
            await _allPricesThrottle.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var result = await ExecuteWithResultAsync(
                    client,
                    () => client.GetAllPricesAsync(cancellationToken),
                    cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess && result.Data is not null)
                {
                    foreach (var price in result.Data)
                    {
                        await channel.Writer.WriteAsync(price, cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    LogExchangeFailure(result, "all prices");
                    if (result.Exception is not null)
                    {
                        errors.TryAdd(client.ExchangeName, result.Exception);
                    }
                }
            }
            finally
            {
                _allPricesThrottle.Release();
            }
        });

        _ = Task.WhenAll(producerTasks).ContinueWith(
            t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "One or more exchange calls failed during price aggregation");
                }
                channel.Writer.Complete();
            },
            TaskScheduler.Default);

        var results = new List<ExchangePrice>();
        await foreach (var price in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(price);
        }

        // Log summary if there were errors
        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Price aggregation completed with {ErrorCount} exchange failures: {Exchanges}",
                errors.Count,
                string.Join(", ", errors.Keys));
        }

        return results;
    }

    private async Task<IReadOnlyList<CryptoSymbol>> FetchAllSymbolsAsync(CancellationToken cancellationToken)
    {
        var tasks = _exchangeClients.Select(client =>
            ExecuteWithResultAsync(
                client,
                () => client.GetSymbolsAsync(cancellationToken),
                cancellationToken));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var successResults = results.Where(r => r.IsSuccess && r.Data is not null).ToList();

        foreach (var failure in results.Where(r => !r.IsSuccess))
        {
            LogExchangeFailure(failure, "symbols");
        }

        var commonSymbols = successResults
            .SelectMany(r => r.Data!)
            .GroupBy(s => s.Symbol)
            .Where(g => g.Count() >= 2)
            .Select(g => g.First())
            .OrderBy(s => s.Symbol)
            .ToList();

        _logger.LogDebug(
            "Found {Count} common symbols across {ExchangeCount} exchanges",
            commonSymbols.Count,
            successResults.Count);

        return commonSymbols.AsReadOnly();
    }

    private async Task<ExchangeCallResult<T>> ExecuteWithResultAsync<T>(
        IExchangeClient client,
        Func<Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DefaultTimeout);

            var result = await operation().ConfigureAwait(false);
            stopwatch.Stop();

            return ExchangeCallResult<T>.Success(client.ExchangeName, result, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.Cancelled,
                "Operation was cancelled by caller",
                duration: stopwatch.Elapsed);
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.Timeout,
                $"Request timed out after {DefaultTimeout.TotalSeconds}s",
                ex,
                stopwatch.Elapsed);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.RateLimited,
                "Rate limited by exchange",
                ex,
                stopwatch.Elapsed);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.ServiceUnavailable,
                "Exchange service unavailable",
                ex,
                stopwatch.Elapsed);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.NetworkError,
                ex.Message,
                ex,
                stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return ExchangeCallResult<T>.Failure(
                client.ExchangeName,
                ExchangeErrorType.Unknown,
                ex.Message,
                ex,
                stopwatch.Elapsed);
        }
    }

    private void LogExchangeFailure<T>(ExchangeCallResult<T> result, string context)
    {
        var logLevel = result.ErrorType switch
        {
            ExchangeErrorType.Cancelled => LogLevel.Debug,
            ExchangeErrorType.Timeout => LogLevel.Warning,
            ExchangeErrorType.RateLimited => LogLevel.Warning,
            ExchangeErrorType.SymbolNotFound => LogLevel.Debug,
            _ => LogLevel.Warning
        };

        _logger.Log(
            logLevel,
            result.Exception,
            "[{Exchange}] Failed to get {Context}: {ErrorType} - {Message} (Duration: {Duration}ms)",
            result.ExchangeName,
            context,
            result.ErrorType,
            result.ErrorMessage,
            result.Duration.TotalMilliseconds);
    }

    private static void ValidateSymbol(string symbol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        if (symbol.Length < 2 || symbol.Length > 20)
        {
            throw new ArgumentException($"Invalid symbol length: {symbol.Length}. Must be between 2 and 20 characters.", nameof(symbol));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _allPricesThrottle.Dispose();
        _logger.LogInformation("LiquidityAggregator disposed");
    }
}

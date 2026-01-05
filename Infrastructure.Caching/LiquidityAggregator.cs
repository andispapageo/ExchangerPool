using Domain.Core.Entities;
using Domain.Core.Entities.Aggregates;
using Domain.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Caching;

public sealed class LiquidityAggregator : ILiquidityAggregator
{
    private readonly IEnumerable<IExchangeClient> _exchangeClients;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LiquidityAggregator> _logger;

    private static readonly TimeSpan PriceCacheDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan SymbolCacheDuration = TimeSpan.FromMinutes(5);

    public LiquidityAggregator(
        IEnumerable<IExchangeClient> exchangeClients,
        IMemoryCache cache,
        ILogger<LiquidityAggregator> logger)
    {
        _exchangeClients = exchangeClients;
        _cache = cache;
        _logger = logger;

        _logger.LogInformation("LiquidityAggregator initialized with {Count} exchange clients: {Exchanges}",
            _exchangeClients.Count(),
            string.Join(", ", _exchangeClients.Select(c => c.ExchangeName)));
    }

    public async Task<AggregatedPrice> GetBestPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"price_{symbol.ToUpperInvariant()}";

        if (_cache.TryGetValue(cacheKey, out AggregatedPrice? cached) && cached is not null)
            return cached;

        var tasks = _exchangeClients.Select(c => GetPriceSafeAsync(c, symbol, cancellationToken));
        var prices = await Task.WhenAll(tasks);

        var validPrices = prices.Where(p => p is not null).Cast<ExchangePrice>().ToList();

        if (validPrices.Count == 0)
        {
            throw new InvalidOperationException($"No prices available for symbol {symbol}");
        }

        var aggregated = AggregatedPrice.Create(symbol, validPrices);
        _cache.Set(cacheKey, aggregated, PriceCacheDuration);

        return aggregated;
    }

    public async Task<IEnumerable<AggregatedPrice>> GetAllBestPricesAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _exchangeClients.Select(c => GetAllPricesSafeAsync(c, cancellationToken));
        var allPrices = await Task.WhenAll(tasks);

        var pricesBySymbol = allPrices
            .SelectMany(p => p)
            .GroupBy(p => p.Symbol)
            .Where(g => g.Count() > 1);

        return pricesBySymbol
            .Select(g => AggregatedPrice.Create(g.Key, g))
            .ToList();
    }

    public async Task<IEnumerable<CryptoSymbol>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "all_symbols";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<CryptoSymbol>? cached) && cached is not null)
            return cached;

        var tasks = _exchangeClients.Select(c => GetSymbolsSafeAsync(c, cancellationToken));
        var allSymbols = await Task.WhenAll(tasks);

        var commonSymbols = allSymbols
            .SelectMany(s => s)
            .GroupBy(s => s.Symbol)
            .Where(g => g.Count() >= 2)
            .Select(g => g.First())
            .OrderBy(s => s.Symbol)
            .ToList();

        _cache.Set(cacheKey, commonSymbols, SymbolCacheDuration);

        return commonSymbols;
    }

    public async Task<IEnumerable<AggregatedPrice>> GetArbitrageOpportunitiesAsync(CancellationToken cancellationToken = default)
    {
        var allPrices = await GetAllBestPricesAsync(cancellationToken);

        return allPrices
            .Where(p => p.HasArbitrageOpportunity)
            .OrderByDescending(p => p.ArbitrageOpportunity)
            .ToList();
    }

    private async Task<ExchangePrice?> GetPriceSafeAsync(
        IExchangeClient client,
        string symbol,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetPriceAsync(symbol, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get price from {Exchange} for {Symbol}",
                client.ExchangeName, symbol);
            return null;
        }
    }

    private async Task<IEnumerable<ExchangePrice>> GetAllPricesSafeAsync(
        IExchangeClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetAllPricesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get all prices from {Exchange}", client.ExchangeName);
            return [];
        }
    }

    private async Task<IEnumerable<CryptoSymbol>> GetSymbolsSafeAsync(
        IExchangeClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            return await client.GetSymbolsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get symbols from {Exchange}", client.ExchangeName);
            return [];
        }
    }
}

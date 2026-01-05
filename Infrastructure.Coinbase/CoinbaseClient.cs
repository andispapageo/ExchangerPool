using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.Coinbase.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.Coinbase;

public sealed class CoinbaseClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinbaseClient> _logger;
    private readonly EndpointOptions _endpoints;

    private static readonly string[] QuoteCurrencies = ["USDT", "USDC", "EUR", "GBP", "BTC", "ETH", "USD"];
    public string ExchangeName => "Coinbase";

    public CoinbaseClient(
        HttpClient httpClient,
        ILogger<CoinbaseClient> logger,
        IOptionsMonitor<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("Coinbase").Endpoints;

        _logger.LogDebug("CoinbaseClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<CoinbaseProductResponse>>(
                _endpoints.Symbols, cancellationToken);

            if (response is null)
                return [];

            return response
                .Where(p => p.Status == "online")
                .Select(p => new CryptoSymbol(
                    p.Id.Replace("-", ""),
                    p.BaseCurrency,
                    p.QuoteCurrency))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from Coinbase");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedSymbol = FormatSymbol(symbol);
            var tickerEndpoint = _endpoints.Ticker.Replace("{symbol}", formattedSymbol);

            _logger.LogDebug("Coinbase: Fetching price from endpoint: {Endpoint}", tickerEndpoint);

            var tickerResponse = await _httpClient.GetFromJsonAsync<CoinbaseTickerResponse>(
                tickerEndpoint, cancellationToken);

            if (tickerResponse is null)
                return null;

            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: symbol.ToUpperInvariant(),
                bidPrice: decimal.Parse(tickerResponse.Bid),
                askPrice: decimal.Parse(tickerResponse.Ask),
                lastPrice: decimal.Parse(tickerResponse.Price),
                volume24H: decimal.Parse(tickerResponse.Volume),
                timestamp: DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from Coinbase", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        var symbols = await GetSymbolsAsync(cancellationToken);
        var tasks = symbols.Take(50).Select(s => GetPriceAsync(s.Symbol, cancellationToken));
        var results = await Task.WhenAll(tasks);

        return results.Where(p => p is not null).Cast<ExchangePrice>();
    }

    private static string FormatSymbol(string symbol)
    {
        foreach (var quote in QuoteCurrencies)
            if (symbol.EndsWith(quote, StringComparison.OrdinalIgnoreCase))
            {
                var baseAsset = symbol[..^quote.Length];
                return $"{baseAsset}-{quote}";
            }
        return symbol;
    }
}
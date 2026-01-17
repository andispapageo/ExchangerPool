using Domain.Core.Entities.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.Kraken.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.Kraken;

public sealed class KrakenClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KrakenClient> _logger;
    private readonly EndpointOptions _endpoints;

    public string ExchangeName => "Kraken";

    public KrakenClient(
        HttpClient httpClient,
        ILogger<KrakenClient> logger,
        IOptionsMonitor<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("Kraken").Endpoints;

        _logger.LogDebug("KrakenClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<KrakenResponse<Dictionary<string, KrakenAssetPair>>>(
                _endpoints.Symbols, cancellationToken);

            if (response?.Result is null)
                return [];

            return response.Result
                .Select(kvp => new CryptoSymbol(
                    kvp.Key,
                    kvp.Value.Base,
                    kvp.Value.Quote))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from Kraken");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedSymbol = FormatSymbol(symbol);
            var endpoint = _endpoints.Ticker.Replace("{symbol}", formattedSymbol);

            _logger.LogDebug("Kraken: Fetching price from endpoint: {Endpoint}", endpoint);

            var response = await _httpClient.GetFromJsonAsync<KrakenResponse<Dictionary<string, KrakenTicker>>>(
                endpoint, cancellationToken);

            if (response?.Result is null || response.Result.Count == 0)
                return null;

            var ticker = response.Result.First();
            return MapToExchangePrice(symbol, ticker.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from Kraken", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<KrakenResponse<Dictionary<string, KrakenTicker>>>(
                _endpoints.AllTickers, cancellationToken);

            if (response?.Result is null)
                return [];

            return response.Result
                .Select(kvp => MapToExchangePrice(kvp.Key, kvp.Value))
                .Where(p => p is not null)
                .Cast<ExchangePrice>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all prices from Kraken");
            return [];
        }
    }

    private static string FormatSymbol(string symbol)
    {
        return symbol.ToUpperInvariant();
    }

    private ExchangePrice? MapToExchangePrice(string symbol, KrakenTicker ticker)
    {
        try
        {
            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: symbol,
                bidPrice: decimal.Parse(ticker.Bid[0]),
                askPrice: decimal.Parse(ticker.Ask[0]),
                lastPrice: decimal.Parse(ticker.Last[0]),
                volume24H: decimal.Parse(ticker.Volume[1]),
                timestamp: DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}
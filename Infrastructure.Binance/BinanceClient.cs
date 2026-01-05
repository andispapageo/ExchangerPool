using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.Binance.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.Binance;

public sealed class BinanceClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceClient> _logger;
    private readonly EndpointOptions _endpoints;
    public string ExchangeName => "Binance";

    public BinanceClient(
        HttpClient httpClient,
        ILogger<BinanceClient> logger,
        IOptionsSnapshot<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("Binance").Endpoints;
        

        _logger.LogDebug("BinanceClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BinanceExchangeInfoResponse>(
                _endpoints.Symbols, cancellationToken);

            if (response?.Symbols is null)
                return [];

            return response.Symbols
                .Where(s => s.Status == "TRADING")
                .Select(s => new CryptoSymbol(s.Symbol, s.BaseAsset, s.QuoteAsset))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from Binance");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedSymbol = symbol.ToUpperInvariant();
            var endpoint = _endpoints.Ticker.Replace("{symbol}", formattedSymbol);

            _logger.LogDebug("Binance: Fetching price from endpoint: {Endpoint}", endpoint);

            var response = await _httpClient.GetFromJsonAsync<BinanceTickerResponse>(
                endpoint, cancellationToken);

            return response is null ? null : MapToExchangePrice(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from Binance", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<BinanceTickerResponse>>(
                _endpoints.AllTickers, cancellationToken);

            if (response is null)
                return [];

            return response
                .Select(MapToExchangePrice)
                .Where(p => p is not null)
                .Cast<ExchangePrice>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all prices from Binance");
            return [];
        }
    }

    private ExchangePrice? MapToExchangePrice(BinanceTickerResponse ticker)
    {
        try
        {
            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: ticker.Symbol,
                bidPrice: decimal.Parse(ticker.BidPrice),
                askPrice: decimal.Parse(ticker.AskPrice),
                lastPrice: decimal.Parse(ticker.LastPrice),
                volume24H: decimal.Parse(ticker.Volume),
                timestamp: DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}
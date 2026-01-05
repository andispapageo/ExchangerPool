using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.Bybit.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.Bybit;

public sealed class BybitClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BybitClient> _logger;
    private readonly EndpointOptions _endpoints;

    public string ExchangeName => "Bybit";

    public BybitClient(
        HttpClient httpClient,
        ILogger<BybitClient> logger,
        IOptionsMonitor<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("Bybit").Endpoints;

        _logger.LogDebug("BybitClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BybitResponse<BybitSymbolResult>>(
                _endpoints.Symbols, cancellationToken);

            if (response?.Result?.List is null)
                return [];

            return response.Result.List
                .Where(s => s.Status == "Trading")
                .Select(s => new CryptoSymbol(s.Symbol, s.BaseCoin, s.QuoteCoin))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from Bybit");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = _endpoints.Ticker.Replace("{symbol}", symbol.ToUpperInvariant());

            _logger.LogDebug("Bybit: Fetching price from endpoint: {Endpoint}", endpoint);

            var response = await _httpClient.GetFromJsonAsync<BybitResponse<BybitTickerResult>>(
                endpoint, cancellationToken);

            var ticker = response?.Result?.List?.FirstOrDefault();
            return ticker is null ? null : MapToExchangePrice(ticker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from Bybit", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BybitResponse<BybitTickerResult>>(
                _endpoints.AllTickers, cancellationToken);

            if (response?.Result?.List is null)
                return [];

            return response.Result.List
                .Select(MapToExchangePrice)
                .Where(p => p is not null)
                .Cast<ExchangePrice>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all prices from Bybit");
            return [];
        }
    }

    private ExchangePrice? MapToExchangePrice(BybitTicker ticker)
    {
        try
        {
            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: ticker.Symbol,
                bidPrice: decimal.Parse(ticker.Bid1Price),
                askPrice: decimal.Parse(ticker.Ask1Price),
                lastPrice: decimal.Parse(ticker.LastPrice),
                volume24H: decimal.Parse(ticker.Volume24h),
                timestamp: DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}
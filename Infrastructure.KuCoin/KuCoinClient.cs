using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.KuCoin.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.KuCoin;

public sealed class KuCoinClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KuCoinClient> _logger;
    private readonly EndpointOptions _endpoints;

    private static readonly string[] QuoteCurrencies = ["USDT", "USDC", "EUR", "BTC", "ETH", "USD"];

    public string ExchangeName => "KuCoin";

    public KuCoinClient(
        HttpClient httpClient,
        ILogger<KuCoinClient> logger,
        IOptionsMonitor<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("KuCoin").Endpoints;

        _logger.LogDebug("KuCoinClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<KuCoinResponse<IReadOnlyList<KuCoinSymbol>>>(
                _endpoints.Symbols, cancellationToken);

            if (response?.Data is null)
                return [];

            return response.Data
                .Where(s => s.EnableTrading)
                .Select(s => new CryptoSymbol(
                    s.Symbol.Replace("-", ""),
                    s.BaseCurrency,
                    s.QuoteCurrency))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from KuCoin");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedSymbol = FormatSymbol(symbol);
            var endpoint = _endpoints.Ticker.Replace("{symbol}", formattedSymbol);

            _logger.LogDebug("KuCoin: Fetching price for {Symbol} -> {FormattedSymbol}", symbol, formattedSymbol);

            var response = await _httpClient.GetFromJsonAsync<KuCoinResponse<KuCoinTicker>>(
                endpoint, cancellationToken);

            if (response is null)
            {
                _logger.LogWarning("KuCoin: Null response for {Symbol}", symbol);
                return null;
            }

            if (response.Code != "200000")
            {
                _logger.LogWarning("KuCoin: API error for {Symbol} - Code: {Code}", symbol, response.Code);
                return null;
            }

            return response.Data is null ? null : MapToExchangePrice(symbol, response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from KuCoin", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<KuCoinResponse<KuCoinAllTickers>>(
                _endpoints.AllTickers, cancellationToken);

            if (response?.Data?.Ticker is null)
                return [];

            return response.Data.Ticker
                .Select(t => MapToExchangePrice(t.Symbol.Replace("-", ""), t))
                .Where(p => p is not null)
                .Cast<ExchangePrice>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all prices from KuCoin");
            return [];
        }
    }

    private static string FormatSymbol(string symbol)
    {
        var upperSymbol = symbol.ToUpperInvariant();
        foreach (var quote in QuoteCurrencies)
        {
            if (upperSymbol.EndsWith(quote, StringComparison.Ordinal))
            {
                var baseAsset = upperSymbol[..^quote.Length];
                if (!string.IsNullOrEmpty(baseAsset))
                {
                    return $"{baseAsset}-{quote}";
                }
            }
        }

        return symbol;
    }

    private ExchangePrice? MapToExchangePrice(string symbol, KuCoinTicker ticker)
    {
        try
        {
            if (string.IsNullOrEmpty(ticker.Buy) ||
                string.IsNullOrEmpty(ticker.Sell) ||
                string.IsNullOrEmpty(ticker.Last))
            {
                return null;
            }

            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: symbol.Replace("-", "").ToUpperInvariant(),
                bidPrice: decimal.Parse(ticker.Buy),
                askPrice: decimal.Parse(ticker.Sell),
                lastPrice: decimal.Parse(ticker.Last),
                volume24H: string.IsNullOrEmpty(ticker.Vol) ? 0 : decimal.Parse(ticker.Vol),
                timestamp: DateTime.UtcNow);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
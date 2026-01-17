using Domain.Core.Entities.Entities;
using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.OKX.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infrastructure.OKX;

public sealed class OKXClient : IExchangeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OKXClient> _logger;
    private readonly EndpointOptions _endpoints;

    private static readonly string[] QuoteCurrencies = ["USDT", "USDC", "USDK", "EUR", "BTC", "ETH", "OKB"];

    public string ExchangeName => "OKX";

    public OKXClient(
        HttpClient httpClient,
        ILogger<OKXClient> logger,
        IOptionsMonitor<ExchangeOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _endpoints = options.Get("OKX").Endpoints;

        _logger.LogDebug("OKXClient initialized with Ticker endpoint: {Ticker}", _endpoints.Ticker);
    }

    public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<OKXResponse<OKXInstrument>>(
                _endpoints.Symbols, cancellationToken);

            if (response?.Data is null)
                return [];

            return response.Data
                .Where(s => s.State == "live")
                .Select(s => new CryptoSymbol(
                    s.InstId.Replace("-", ""),
                    s.BaseCcy,
                    s.QuoteCcy))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get symbols from OKX");
            return [];
        }
    }

    public async Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            var formattedSymbol = FormatSymbol(symbol);

            if (formattedSymbol.EndsWith("-USD", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("OKX: Skipping {Symbol} - USD fiat pairs not supported", symbol);
                return null;
            }

            var endpoint = _endpoints.Ticker.Replace("{symbol}", formattedSymbol);

            _logger.LogDebug("OKX: Fetching price from endpoint: {Endpoint}", endpoint);

            var response = await _httpClient.GetFromJsonAsync<OKXResponse<OKXTicker>>(
                endpoint, cancellationToken);

            if (response?.Code != "0")
            {
                _logger.LogDebug("OKX: Symbol {Symbol} not found or error - Code: {Code}",
                    symbol, response?.Code);
                return null;
            }

            var ticker = response?.Data?.FirstOrDefault();
            return ticker is null ? null : MapToExchangePrice(ticker);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("OKX: Symbol {Symbol} not found", symbol);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get price for {Symbol} from OKX", symbol);
            return null;
        }
    }

    public async Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<OKXResponse<OKXTicker>>(
                _endpoints.AllTickers, cancellationToken);

            if (response?.Data is null)
                return [];

            return response.Data
                .Select(MapToExchangePrice)
                .Where(p => p is not null)
                .Cast<ExchangePrice>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all prices from OKX");
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

    private ExchangePrice? MapToExchangePrice(OKXTicker ticker)
    {
        try
        {
            if (string.IsNullOrEmpty(ticker.BidPx) ||
                string.IsNullOrEmpty(ticker.AskPx) ||
                string.IsNullOrEmpty(ticker.Last))
            {
                return null;
            }

            return new ExchangePrice(
                exchangeName: ExchangeName,
                symbol: ticker.InstId.Replace("-", "").ToUpperInvariant(),
                bidPrice: decimal.Parse(ticker.BidPx),
                askPrice: decimal.Parse(ticker.AskPx),
                lastPrice: decimal.Parse(ticker.Last),
                volume24H: string.IsNullOrEmpty(ticker.Vol24h) ? 0 : decimal.Parse(ticker.Vol24h),
                timestamp: DateTime.UtcNow);
        }
        catch
        {
            return null;
        }
    }
}
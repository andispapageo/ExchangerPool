using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Infrastructure.Coinbase.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Coinbase
{
    public sealed class CoinbaseClient : IExchangeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CoinbaseClient> _logger;

        private static readonly string[] QuoteCurrencies = ["USD", "USDT", "USDC", "EUR", "GBP", "BTC", "ETH"];

        public string ExchangeName => "Coinbase";

        public CoinbaseClient(HttpClient httpClient, ILogger<CoinbaseClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IReadOnlyList<CoinbaseProductResponse>>(
                    "products", cancellationToken);

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

                var tickerTask = _httpClient.GetFromJsonAsync<CoinbaseTickerResponse>(
                    $"products/{formattedSymbol}/ticker", cancellationToken);

                var statsTask = _httpClient.GetFromJsonAsync<CoinbaseStatsResponse>(
                    $"products/{formattedSymbol}/stats", cancellationToken);

                await Task.WhenAll(tickerTask, statsTask);

                var ticker = await tickerTask;
                var stats = await statsTask;

                if (ticker is null || stats is null)
                    return null;

                return new ExchangePrice(
                    exchangeName: ExchangeName,
                    symbol: symbol.ToUpperInvariant(),
                    bidPrice: decimal.Parse(ticker.Bid),
                    askPrice: decimal.Parse(ticker.Ask),
                    lastPrice: decimal.Parse(ticker.Price),
                    volume24H: decimal.Parse(stats.Volume),
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
            {
                if (symbol.EndsWith(quote, StringComparison.OrdinalIgnoreCase))
                {
                    var baseAsset = symbol[..^quote.Length];
                    return $"{baseAsset}-{quote}";
                }
            }
            return symbol;
        }
    }
}
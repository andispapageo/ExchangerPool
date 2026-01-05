using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Infrastructure.Kraken.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Infrastructure.Kraken
{
    internal class KrakenClient : IExchangeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KrakenClient> _logger;

        public string ExchangeName => "Kraken";

        public KrakenClient(HttpClient httpClient, ILogger<KrakenClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<KrakenResponse<Dictionary<string, KrakenAssetPair>>>(
                    "0/public/AssetPairs", cancellationToken);

                if (response?.Result is null)
                    return [];

                return response.Result
                    .Select(kvp => new CryptoSymbol(kvp.Key, kvp.Value.Base, kvp.Value.Quote))
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
                var response = await _httpClient.GetFromJsonAsync<KrakenResponse<Dictionary<string, KrakenTicker>>>(
                    $"0/public/Ticker?pair={symbol}", cancellationToken);

                if (response?.Result is null || response.Result.Count == 0)
                    return null;

                var ticker = response.Result.First();
                return MapToExchangePrice(ticker.Key, ticker.Value);
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
                    "0/public/Ticker", cancellationToken);

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
}

using Domain.Core.Entities;
using Domain.Core.Interfaces;
using Infrastructure.Binance.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Binance
{
    public sealed class BinanceClient : IExchangeClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BinanceClient> _logger;

        public string ExchangeName => "Binance";

        public BinanceClient(HttpClient httpClient, ILogger<BinanceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<BinanceExchangeInfoResponse>(
                    "api/v3/exchangeInfo", cancellationToken);

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
                var response = await _httpClient.GetFromJsonAsync<BinanceTickerResponse>(
                    $"api/v3/ticker/24hr?symbol={symbol.ToUpperInvariant()}", cancellationToken);

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
                    "api/v3/ticker/24hr", cancellationToken);

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
}

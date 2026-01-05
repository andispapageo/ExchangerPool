using System.Text.Json.Serialization;
namespace Infrastructure.Binance.Models;
public sealed record BinanceTickerResponse(
[property: JsonPropertyName("symbol")] string Symbol,
[property: JsonPropertyName("bidPrice")] string BidPrice,
[property: JsonPropertyName("askPrice")] string AskPrice,
[property: JsonPropertyName("lastPrice")] string LastPrice,
[property: JsonPropertyName("volume")] string Volume);

public sealed record BinanceExchangeInfoResponse(
    [property: JsonPropertyName("symbols")] IReadOnlyList<BinanceSymbolInfo> Symbols);

public sealed record BinanceSymbolInfo(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("baseAsset")] string BaseAsset,
    [property: JsonPropertyName("quoteAsset")] string QuoteAsset);

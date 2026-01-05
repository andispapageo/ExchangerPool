using System.Text.Json.Serialization;
namespace Infrastructure.KuCoin.Models;

public sealed record KuCoinResponse<T>(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("data")] T? Data);

public sealed record KuCoinTicker(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("buy")] string Buy,
    [property: JsonPropertyName("sell")] string Sell,
    [property: JsonPropertyName("last")] string Last,
    [property: JsonPropertyName("vol")] string Vol);

public sealed record KuCoinSymbol(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("baseCurrency")] string BaseCurrency,
    [property: JsonPropertyName("quoteCurrency")] string QuoteCurrency,
    [property: JsonPropertyName("enableTrading")] bool EnableTrading);

public sealed record KuCoinAllTickers(
    [property: JsonPropertyName("ticker")] IReadOnlyList<KuCoinTicker> Ticker);
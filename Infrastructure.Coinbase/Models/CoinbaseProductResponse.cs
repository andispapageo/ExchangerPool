using System.Text.Json.Serialization;

namespace Infrastructure.Coinbase.Models;
public sealed record CoinbaseProductResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("base_currency")] string BaseCurrency,
    [property: JsonPropertyName("quote_currency")] string QuoteCurrency,
    [property: JsonPropertyName("status")] string Status);

public sealed record CoinbaseTickerResponse(
    [property: JsonPropertyName("bid")] string Bid,
    [property: JsonPropertyName("ask")] string Ask,
    [property: JsonPropertyName("price")] string Price,
    [property: JsonPropertyName("volume")] string Volume);
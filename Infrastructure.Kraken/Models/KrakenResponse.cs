using System.Text.Json.Serialization;

namespace Infrastructure.Kraken.Models;

public sealed record KrakenResponse<T>(
    [property: JsonPropertyName("error")] IReadOnlyList<string> Error,
    [property: JsonPropertyName("result")] T? Result);

public sealed record KrakenAssetPair(
    [property: JsonPropertyName("base")] string Base,
    [property: JsonPropertyName("quote")] string Quote);

public sealed record KrakenTicker(
    [property: JsonPropertyName("a")] IReadOnlyList<string> Ask,   // [price, wholeLotVolume, lotVolume]
    [property: JsonPropertyName("b")] IReadOnlyList<string> Bid,   // [price, wholeLotVolume, lotVolume]
    [property: JsonPropertyName("c")] IReadOnlyList<string> Last,  // [price, lotVolume]
    [property: JsonPropertyName("v")] IReadOnlyList<string> Volume); // [today, 24h]

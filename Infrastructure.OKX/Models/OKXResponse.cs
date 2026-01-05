using System.Text.Json.Serialization;

namespace Infrastructure.OKX.Models;
public sealed record OKXResponse<T>(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("msg")] string Msg,
    [property: JsonPropertyName("data")] IReadOnlyList<T>? Data);

public sealed record OKXTicker(
    [property: JsonPropertyName("instId")] string InstId,
    [property: JsonPropertyName("bidPx")] string BidPx,
    [property: JsonPropertyName("askPx")] string AskPx,
    [property: JsonPropertyName("last")] string Last,
    [property: JsonPropertyName("vol24h")] string Vol24h);

public sealed record OKXInstrument(
    [property: JsonPropertyName("instId")] string InstId,
    [property: JsonPropertyName("baseCcy")] string BaseCcy,
    [property: JsonPropertyName("quoteCcy")] string QuoteCcy,
    [property: JsonPropertyName("state")] string State);
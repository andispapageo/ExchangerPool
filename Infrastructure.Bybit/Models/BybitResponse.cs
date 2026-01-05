using System.Text.Json.Serialization;
namespace Infrastructure.Bybit.Models;
public sealed record BybitResponse<T>(
    [property: JsonPropertyName("retCode")] int RetCode,
    [property: JsonPropertyName("retMsg")] string RetMsg,
    [property: JsonPropertyName("result")] T? Result);

public sealed record BybitTickerResult(
    [property: JsonPropertyName("list")] IReadOnlyList<BybitTicker> List);

public sealed record BybitTicker(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("bid1Price")] string Bid1Price,
    [property: JsonPropertyName("ask1Price")] string Ask1Price,
    [property: JsonPropertyName("lastPrice")] string LastPrice,
    [property: JsonPropertyName("volume24h")] string Volume24h);

public sealed record BybitSymbolResult(
    [property: JsonPropertyName("list")] IReadOnlyList<BybitSymbol> List);

public sealed record BybitSymbol(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("baseCoin")] string BaseCoin,
    [property: JsonPropertyName("quoteCoin")] string QuoteCoin,
    [property: JsonPropertyName("status")] string Status);
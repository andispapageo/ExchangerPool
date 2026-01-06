namespace Application.Common.DTOs
{
    public sealed record AggregatedPriceDto(
    string Symbol,
    decimal BestBidPrice,
    string BestBidExchange,
    decimal BestAskPrice,
    string BestAskExchange,
    decimal Spread,
    decimal ArbitrageOpportunityPercent,
    bool HasArbitrageOpportunity,
    DateTime AggregatedAt,
    IReadOnlyList<ExchangePriceDto> AllPrices);

    public sealed record ExchangePriceDto(
        string Exchange,
        string Symbol,
        decimal BidPrice,
        decimal AskPrice,
        decimal LastPrice,
        decimal Volume24H,
        decimal SpreadPercent,
        DateTime Timestamp);

    public sealed record CryptoSymbolDto(
        string Symbol,
        string BaseAsset,
        string QuoteAsset);
}

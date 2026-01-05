using Application.Common.DTOs;
using Domain.Core.Entities;
using Domain.Core.Entities.Aggregates;

namespace Application.Common.Mappings
{
    public static class DtoMapper
    {
        public static AggregatedPriceDto ToDto(this AggregatedPrice aggregate) =>
          new(
              Symbol: aggregate.Symbol,
              BestBidPrice: aggregate.BestBid.BidPrice,
              BestBidExchange: aggregate.BestBid.ExchangeName,
              BestAskPrice: aggregate.BestAsk.AskPrice,
              BestAskExchange: aggregate.BestAsk.ExchangeName,
              Spread: aggregate.BestAsk.AskPrice - aggregate.BestBid.BidPrice,
              ArbitrageOpportunityPercent: aggregate.ArbitrageOpportunity,
              HasArbitrageOpportunity: aggregate.HasArbitrageOpportunity,
              AggregatedAt: aggregate.AggregatedAt,
              AllPrices: aggregate.AllPrices.Select(p => p.ToDto()).ToList());

        public static ExchangePriceDto ToDto(this ExchangePrice price) =>
            new(
                Exchange: price.ExchangeName,
                Symbol: price.Symbol,
                BidPrice: price.BidPrice,
                AskPrice: price.AskPrice,
                LastPrice: price.LastPrice,
                Volume24H: price.Volume24H,
                SpreadPercent: price.SpreadPercentage,
                Timestamp: price.Timestamp);

        public static CryptoSymbolDto ToDto(this CryptoSymbol symbol) =>
            new(
                Symbol: symbol.Symbol,
                BaseAsset: symbol.BaseAsset,
                QuoteAsset: symbol.QuoteAsset);
    }
}

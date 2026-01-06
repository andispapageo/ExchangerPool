namespace Domain.Core.Entities.Aggregates
{
    public sealed class AggregatedPrice
    {
        public string Symbol { get; }
        public ExchangePrice BestBid { get; }
        public ExchangePrice BestAsk { get; }
        public decimal ArbitrageOpportunity { get; }
        public IReadOnlyList<ExchangePrice> AllPrices { get; }
        public DateTime AggregatedAt { get; }

        private AggregatedPrice(
            string symbol,
            ExchangePrice bestBid,
            ExchangePrice bestAsk,
            IReadOnlyList<ExchangePrice> allPrices)
        {
            Symbol = symbol;
            BestBid = bestBid;
            BestAsk = bestAsk;
            AllPrices = allPrices;
            AggregatedAt = DateTime.UtcNow;
            ArbitrageOpportunity = CalculateArbitrage(bestBid, bestAsk);
        }

        public static AggregatedPrice Create(string symbol, IEnumerable<ExchangePrice> prices)
        {
            var priceList = prices.ToList();

            if (priceList.Count == 0)
                throw new InvalidOperationException($"No prices available for symbol {symbol}");

            var bestBid = priceList.MaxBy(p => p.BidPrice)!;
            var bestAsk = priceList.MinBy(p => p.AskPrice)!;

            return new AggregatedPrice(symbol, bestBid, bestAsk, priceList.AsReadOnly());
        }

        private static decimal CalculateArbitrage(ExchangePrice bestBid, ExchangePrice bestAsk)
        {
            if (bestAsk.AskPrice > 0)
                return ((bestBid.BidPrice - bestAsk.AskPrice) / bestAsk.AskPrice) * 100;
            return 0;
        }

        public bool HasArbitrageOpportunity => ArbitrageOpportunity > 0;
    }
}

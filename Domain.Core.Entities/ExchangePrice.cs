namespace Domain.Core.Entities
{
    public sealed class ExchangePrice
    {
        public string ExchangeName { get; }
        public string Symbol { get; }
        public decimal BidPrice { get; }
        public decimal AskPrice { get; }
        public decimal LastPrice { get; }
        public decimal Volume24H { get; }
        public DateTime Timestamp { get; }
        public decimal Spread => AskPrice - BidPrice;
        public decimal SpreadPercentage => BidPrice > 0 ? (Spread / BidPrice) * 100 : 0;
        public ExchangePrice(
            string exchangeName,
            string symbol,
            decimal bidPrice,
            decimal askPrice,
            decimal lastPrice,
            decimal volume24H,
            DateTime timestamp)
        {
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            Symbol = symbol?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(symbol));
            BidPrice = bidPrice >= 0 ? bidPrice : throw new ArgumentException("Bid price cannot be negative");
            AskPrice = askPrice >= 0 ? askPrice : throw new ArgumentException("Ask price cannot be negative");
            LastPrice = lastPrice >= 0 ? lastPrice : throw new ArgumentException("Last price cannot be negative");
            Volume24H = volume24H >= 0 ? volume24H : throw new ArgumentException("Volume cannot be negative");
            Timestamp = timestamp;
        }
    }
}

namespace Domain.Core.Entities
{
    public sealed class CryptoSymbol
    {
        public string Symbol { get; }
        public string BaseAsset { get; }
        public string QuoteAsset { get; }

        public CryptoSymbol(string symbol, string baseAsset, string quoteAsset)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

            Symbol = symbol.ToUpperInvariant();
            BaseAsset = baseAsset?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(baseAsset));
            QuoteAsset = quoteAsset?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(quoteAsset));
        }
    }
}

namespace ExchangerPool.LiquidityContributors
{
    public class GetBestPriceBySymbolRequest
    {
        public const string Route = "/Liquidity/price/{symbol}";
        public static string BuildRoute(string symbol) => Route.Replace("{symbol}", symbol.ToString());

        [BindFrom("symbol")]
        public string Symbol { get; set; } = string.Empty;
    }
}

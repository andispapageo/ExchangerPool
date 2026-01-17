using Domain.Core.Entities.Aggregates;
using Domain.Core.Entities.Entities;
namespace Domain.Core.Interfaces;
public interface ILiquidityAggregator
{
    Task<AggregatedPrice> GetBestPriceAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IEnumerable<AggregatedPrice>> GetAllBestPricesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CryptoSymbol>> GetAvailableSymbolsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AggregatedPrice>> GetArbitrageRiskAsync(CancellationToken cancellationToken = default);
}

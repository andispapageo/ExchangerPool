using Domain.Core.Entities.Entities;

namespace Domain.Core.Interfaces;
public interface IExchangeClient
{
    string ExchangeName { get; }
    Task<IEnumerable<CryptoSymbol>> GetSymbolsAsync(CancellationToken cancellationToken = default);
    Task<ExchangePrice?> GetPriceAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangePrice>> GetAllPricesAsync(CancellationToken cancellationToken = default);
}

using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;
namespace Application.Common.UseCases;
public sealed record GetAvailableSymbolsUseCase(ILiquidityAggregator aggregator)
{
    public async Task<IEnumerable<CryptoSymbolDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        => (await aggregator.GetAvailableSymbolsAsync(cancellationToken)).Select(r => r.ToDto());
}

using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;
namespace Application.Common.UseCases;
public sealed class GetArbitrageRiskUseCase(ILiquidityAggregator aggregator)
{
    public async Task<IEnumerable<AggregatedPriceDto>> ExecuteAsync(CancellationToken cancellationToken = default) 
        => (await aggregator.GetArbitrageRiskAsync(cancellationToken)).Select(r => r.ToDto());
}

using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;

namespace Application.Common.UseCases
{
    public sealed class GetArbitrageRiskUseCase
    {
        private readonly ILiquidityAggregator _aggregator;
        public GetArbitrageRiskUseCase(ILiquidityAggregator aggregator) => _aggregator = aggregator;
        public async Task<IEnumerable<AggregatedPriceDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var results = await _aggregator.GetArbitrageRiskAsync(cancellationToken);
            return results.Select(r => r.ToDto());
        }
    }
}

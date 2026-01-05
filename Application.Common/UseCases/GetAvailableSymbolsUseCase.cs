using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;

namespace Application.Common.UseCases
{
    public sealed class GetAvailableSymbolsUseCase
    {
        private readonly ILiquidityAggregator _aggregator;
        public GetAvailableSymbolsUseCase(ILiquidityAggregator aggregator) => _aggregator = aggregator;

        public async Task<IEnumerable<CryptoSymbolDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var results = await _aggregator.GetAvailableSymbolsAsync(cancellationToken);
            return results.Select(r => r.ToDto());
        }
    }
}

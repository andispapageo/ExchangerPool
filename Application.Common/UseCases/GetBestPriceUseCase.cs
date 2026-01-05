using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;

namespace Application.Common.UseCases
{
    public sealed class GetBestPriceUseCase
    {
        private readonly ILiquidityAggregator _aggregator;
        public GetBestPriceUseCase(ILiquidityAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        public async Task<AggregatedPriceDto> ExecuteAsync(string symbol, CancellationToken cancellationToken = default)
        {
            var result = await _aggregator.GetBestPriceAsync(symbol, cancellationToken);
            return result.ToDto();
        }
    }
}

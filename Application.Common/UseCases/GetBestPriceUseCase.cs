using Application.Common.DTOs;
using Application.Common.Mappings;
using Domain.Core.Interfaces;
namespace Application.Common.UseCases;
public sealed record GetBestPriceUseCase(ILiquidityAggregator aggregator)
{
    public async Task<AggregatedPriceDto> ExecuteAsync(string symbol, CancellationToken cancellationToken = default)
        => (await aggregator.GetBestPriceAsync(symbol, cancellationToken)).ToDto();
}

using Application.Common.DTOs;
using Application.Common.Mappings;
using Application.Common.Specifications;
using Domain.Core.Entities.Aggregates;
using Domain.Core.Interfaces;
using Domain.Core.Specifications;
namespace Application.Common.UseCases;
public sealed record GetBestPriceUseCase(ILiquidityAggregator aggregator)
{
    public async Task<AggregatedPriceDto?> ExecuteAsync(
           string symbol,
           ISpecification<AggregatedPrice>? specification = null,
           CancellationToken cancellationToken = default)
    {
        var result = await aggregator.GetBestPriceAsync(symbol, cancellationToken);
        if (result is null)
            return null;

        specification ??= new SymbolSpecification(symbol);

        if (!specification.IsSatisfiedBy(result))
            return null;

        return result.ToDto();
    }
}

using Application.Common.DTOs;
using Application.Common.Mappings;
using Application.Common.Specifications;
using Domain.Core.Entities.Aggregates;
using Domain.Core.Interfaces;
namespace Application.Common.UseCases;
public sealed class GetArbitrageRiskUseCase(ILiquidityAggregator aggregator)
{
    public async Task<IEnumerable<AggregatedPriceDto>> ExecuteAsync(
        ISpecification<AggregatedPrice>? specification = null,
        CancellationToken cancellationToken = default)
    {
        var results = await aggregator.GetArbitrageRiskAsync(cancellationToken);
        
        specification ??= new ArbitrageRiskSpecification();
        
        return results
            .Where(r => specification.IsSatisfiedBy(r))
            .OrderByDescending(r => r.ArbitrageRisk)
            .Select(r => r.ToDto());
    }
}

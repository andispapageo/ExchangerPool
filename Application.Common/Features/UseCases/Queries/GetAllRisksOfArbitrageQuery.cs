using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Specifications;
using Domain.Core.Entities.Aggregates;
using Domain.Core.Interfaces;
namespace Application.Common.Features.UseCases.Queries;

public record GetAllRisksOfArbitrageQuery(ISpecification<AggregatedPrice>? Specification = null)
    : IQuery<Result<IEnumerable<AggregatedPriceDto>>>;
public sealed class GetAllRisksOfArbitrageQueryHandler(ILiquidityAggregator aggregator)
   : IQueryHandler<GetAllRisksOfArbitrageQuery, Result<IEnumerable<AggregatedPriceDto>>>
{
    public async Task<Result<IEnumerable<AggregatedPriceDto>>> Handle(
        GetAllRisksOfArbitrageQuery request,
        CancellationToken cancellationToken)
    {
        var results = await aggregator.GetArbitrageRiskAsync(cancellationToken);

        var specification = request.Specification ?? new ArbitrageRiskSpecification();

        var filtered = results
            .Where(r => specification.IsSatisfiedBy(r))
            .OrderByDescending(r => r.ArbitrageRisk)
            .Select(r => r.ToDto());

        return filtered.Any()
            ? Result<IEnumerable<AggregatedPriceDto>>.Success(filtered)
            : Result<IEnumerable<AggregatedPriceDto>>.NotFound("All Risks for Arbitrage failed");
    }
}

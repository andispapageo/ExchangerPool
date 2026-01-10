using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.UseCases;
namespace Application.Common.Features.Queries;

public record GetAllRisksOfArbitrageQuery() : IQuery<Result<IEnumerable<AggregatedPriceDto>>>;
sealed record GetRiskOfArbitrageQueryHandler(GetArbitrageRiskUseCase getArbitrageRiskUseCase)
   : IQueryHandler<GetAllRisksOfArbitrageQuery, Result<IEnumerable<AggregatedPriceDto>>>
{
    public Task<Result<IEnumerable<AggregatedPriceDto>>> Handle(
        GetAllRisksOfArbitrageQuery request,
        CancellationToken cancellationToken) => HandleAsync(request, cancellationToken);
    private async Task<Result<IEnumerable<AggregatedPriceDto>>> HandleAsync(
        GetAllRisksOfArbitrageQuery request,
        CancellationToken cancellationToken)
    {
        var result = await getArbitrageRiskUseCase.ExecuteAsync(cancellationToken);

        return result is not null
            ? Result<IEnumerable<AggregatedPriceDto>>.Success(result)
            : Result<IEnumerable<AggregatedPriceDto>>.NotFound($"All Risks for Arbirtage failed");
    }
}

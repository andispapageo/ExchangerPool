using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Specifications;
using Domain.Core.Interfaces;
namespace Application.Common.Features.Queries;
public record GetBestPricesBySymbolQuery(string Symbol) : IQuery<Result<AggregatedPriceDto>>;

public sealed class GetBestPricesBySymbolQueryHandler(ILiquidityAggregator aggregator)
    : IQueryHandler<GetBestPricesBySymbolQuery, Result<AggregatedPriceDto>>
{
    public Task<Result<AggregatedPriceDto>> Handle(
        GetBestPricesBySymbolQuery request,
        CancellationToken cancellationToken) => HandleAsync(request, cancellationToken);

    private async Task<Result<AggregatedPriceDto>> HandleAsync(
        GetBestPricesBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new SymbolSpecification(request.Symbol);
        var result = await aggregator.GetBestPriceAsync(request.Symbol, cancellationToken);
        if (result is null || !specification.IsSatisfiedBy(result))
        {
            return Result<AggregatedPriceDto>.NotFound($"Symbol '{request.Symbol}' not found");
        }

        return Result<AggregatedPriceDto>.Success(result.ToDto());
    }
}

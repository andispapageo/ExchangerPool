using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.Mappings;
using Domain.Core.Interfaces;

namespace Application.Common.Features.UseCases.Queries;

public record GetAllSymbolsQuery() : IQuery<Result<IEnumerable<CryptoSymbolDto>>>;

public sealed class GetAllSymbolsQueryHandler(ILiquidityAggregator aggregator)
   : IQueryHandler<GetAllSymbolsQuery, Result<IEnumerable<CryptoSymbolDto>>>
{
    public async Task<Result<IEnumerable<CryptoSymbolDto>>> Handle(
        GetAllSymbolsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await aggregator.GetAvailableSymbolsAsync(cancellationToken);

        return result is not null
            ? Result<IEnumerable<CryptoSymbolDto>>.Success(result.Select(r => r.ToDto()))
            : Result<IEnumerable<CryptoSymbolDto>>.NotFound("All symbols request failed");
    }
}

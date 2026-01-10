using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.UseCases;
namespace Application.Common.Features.Queries;
public record GetBestPricesBySymbolQuery(string Symbol) : IQuery<Result<AggregatedPriceDto>>;
 sealed record GetBestPricesBySymbolQueryHandler(GetBestPriceUseCase getBestPriceUseCase)
    : IQueryHandler<GetBestPricesBySymbolQuery, Result<AggregatedPriceDto>>
{
    public Task<Result<AggregatedPriceDto>> Handle(
        GetBestPricesBySymbolQuery request,
        CancellationToken cancellationToken) => HandleAsync(request, cancellationToken);

    private async Task<Result<AggregatedPriceDto>> HandleAsync(
        GetBestPricesBySymbolQuery request,
        CancellationToken cancellationToken)
    {
        var result = await getBestPriceUseCase.ExecuteAsync(request.Symbol, cancellationToken);

        return result is not null
            ? Result<AggregatedPriceDto>.Success(result)
            : Result<AggregatedPriceDto>.NotFound($"Symbol '{request.Symbol}' not found");
    }
}

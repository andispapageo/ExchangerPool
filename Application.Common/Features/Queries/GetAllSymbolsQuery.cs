using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Common.UseCases;
namespace Application.Common.Features.Queries;
public record GetAllSymbolsQuery() : IQuery<Result<IEnumerable<CryptoSymbolDto>>>;
sealed record GetAllSymbolsQueryHandler(GetAvailableSymbolsUseCase getAvailableSymbolsUseCase)
   : IQueryHandler<GetAllSymbolsQuery, Result<IEnumerable<CryptoSymbolDto>>>
{
    public Task<Result<IEnumerable<CryptoSymbolDto>>> Handle(
        GetAllSymbolsQuery request,
        CancellationToken cancellationToken) => HandleAsync(request, cancellationToken);
    private async Task<Result<IEnumerable<CryptoSymbolDto>>> HandleAsync(
        GetAllSymbolsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await getAvailableSymbolsUseCase.ExecuteAsync(cancellationToken);

        return result is not null
            ? Result<IEnumerable<CryptoSymbolDto>>.Success(result)
            : Result<IEnumerable<CryptoSymbolDto>>.NotFound($"All symbols request failed");
    }
}

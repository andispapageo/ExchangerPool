using Application.Common.DTOs;
using Application.Common.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExchangerPool.LiquidityContributors
{
    public class GetAllSymbols(ILogger<GetAllSymbols> logger, IMediator mediator)
         : EndpointWithoutRequest<Results<Ok<IEnumerable<CryptoSymbolDto>>, NotFound, ProblemHttpResult>>
    {
        public override void Configure()
        {
            Get("/Liquidity/symbols");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all the symbols by Liquidity Providers";
                s.Description = "Retrieves all available trading symbols from all liquidity providers.";
                s.ResponseExamples[200] = new List<CryptoSymbolDto>();
                s.Responses[200] = "Symbols found and returned successfully";
                s.Responses[404] = "GetAllSymbols Failed";
            });
            Tags("Liquidity");

            Description(builder => builder
              .Produces<IEnumerable<CryptoSymbolDto>>(200, "application/json")
              .ProducesProblem(404));
        }

        public override async Task<Results<Ok<IEnumerable<CryptoSymbolDto>>, NotFound, ProblemHttpResult>> ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("Started getting all symbols");
            var result = await mediator.Send(new GetAllSymbolsQuery(), ct);

            return result is not null
                ? TypedResults.Ok(result.Value)
                : TypedResults.NotFound();
        }
    }
}

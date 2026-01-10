using Application.Common.DTOs;
using Application.Common.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
namespace ExchangerPool.LiquidityContributors
{
    public class GetBestPrice(ILogger<GetBestPrice> logger, IMediator mediator)
        : Endpoint<GetBestPriceBySymbolRequest,
                 Results<Ok<AggregatedPriceDto>,
                     NotFound,
                     ProblemHttpResult>>
    {
        public override void Configure()
        {
            Get(GetBestPriceBySymbolRequest.Route);
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get the best price for a symbol by Liquidity Providers";
                s.Description = "Retrieves the best price for a given trading symbol from all liquidity providers.";
                s.ExampleRequest = new GetBestPriceBySymbolRequest { Symbol = "BTCUSD" };
                s.ResponseExamples[200] = new AggregatedPriceDto("BTCUSD", 0, "", 0, "", 0, 0, false, DateTime.MinValue, []);
                s.Responses[200] = "Best price found and returned successfully";
                s.Responses[404] = "Symbol not found";
            });
            Tags("Liquidity");

            Description(builder => builder
              .Produces<AggregatedPriceDto>(200, "application/json")
              .ProducesProblem(404));
        }

        public override async Task<Results<Ok<AggregatedPriceDto>, NotFound, ProblemHttpResult>> ExecuteAsync(GetBestPriceBySymbolRequest req, CancellationToken ct)
        {
            logger.LogInformation("Getting best price for {Symbol}", req.Symbol?.ToUpper());
            var result = await mediator.Send(new GetBestPricesBySymbolQuery(req.Symbol?.ToUpper() ?? string.Empty), ct);

            return result is not null
                      ? TypedResults.Ok(result.Value)
                      : TypedResults.NotFound();
        }
    }
}

using Application.Common.DTOs;
using Application.Common.Features.Queries;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExchangerPool.LiquidityContributors
{
    public class GetAllRisksOfArbirtage(ILogger<GetAllRisksOfArbirtage> logger, IMediator mediator)
        : EndpointWithoutRequest<Results<Ok<IEnumerable<AggregatedPriceDto>>, NotFound, ProblemHttpResult>>
    {
        public override void Configure()
        {
            Get("/Liquidity/arbitrage");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Get all arbitrage opportunities across exchanges";
                s.Description = "Retrieves current arbitrage opportunities by comparing prices across all liquidity providers.";
                s.ResponseExamples[200] = new List<AggregatedPriceDto>();
                s.Responses[200] = "Arbitrage opportunities found and returned successfully";
                s.Responses[404] = "No arbitrage opportunities found";
            });
            Tags("Liquidity");

            Description(builder => builder
              .Produces<IEnumerable<AggregatedPriceDto>>(200, "application/json")
              .ProducesProblem(404));
        }


        public override async Task<Results<Ok<IEnumerable<AggregatedPriceDto>>, NotFound, ProblemHttpResult>> ExecuteAsync(CancellationToken ct)
        {
            logger.LogInformation("Started getting all symbols");
            var result = await mediator.Send(new GetAllRisksOfArbitrageQuery(), ct);

            return result is not null
                ? TypedResults.Ok(result.Value)
                : TypedResults.NotFound();
        }
    }
}

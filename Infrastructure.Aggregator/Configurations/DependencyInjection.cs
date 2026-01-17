using Domain.Core.Entities.Aggregates;
using Domain.Core.Entities.Entities;
using Domain.Core.Interfaces;
using Infrastructure.Aggregator.Caching;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure.Aggregator.Config;

public static class DependencyInjection
{
    public static IServiceCollection AddMainAggregator(this IServiceCollection services)
    {
        services.AddSingleton<IAsyncCache<AggregatedPrice>, AsyncCache<AggregatedPrice>>();
        services.AddSingleton<IAsyncCache<IReadOnlyList<CryptoSymbol>>, AsyncCache<IReadOnlyList<CryptoSymbol>>>();
        services.AddScoped<ILiquidityAggregator, LiquidityAggregator>();

        return services;
    }
}

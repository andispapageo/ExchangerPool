using Domain.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure.Caching.Config;

public static class DependencyInjection
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ILiquidityAggregator, LiquidityAggregator>();

        return services;
    }
}

using Application.Common.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace Application.Common.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<GetBestPriceUseCase>();
            services.AddScoped<GetArbitrageOpportunitiesUseCase>();
            services.AddScoped<GetAvailableSymbolsUseCase>();

            return services;
        }
    }
}

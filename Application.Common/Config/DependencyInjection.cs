using Application.Common.Behaviors;
using Application.Common.UseCases;
using Microsoft.Extensions.DependencyInjection;
namespace Application.Common.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            });
            services.AddScoped<GetBestPriceUseCase>();
            services.AddScoped<GetArbitrageRiskUseCase>();
            services.AddScoped<GetAvailableSymbolsUseCase>();

            return services;
        }
    }
}

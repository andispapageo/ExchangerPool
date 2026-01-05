using Domain.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Coinbase.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoinbase(this IServiceCollection services)
        {
            services.AddHttpClient<CoinbaseClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.exchange.coinbase.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddSingleton<IExchangeClient>(sp => sp.GetRequiredService<CoinbaseClient>());

            return services;
        }
    }
}

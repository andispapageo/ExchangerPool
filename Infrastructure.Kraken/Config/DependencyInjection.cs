using Domain.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Kraken.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddKraken(this IServiceCollection services)
        {
            services.AddHttpClient<KrakenClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.kraken.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddSingleton<IExchangeClient>(sp => sp.GetRequiredService<KrakenClient>());

            return services;
        }
    }
}

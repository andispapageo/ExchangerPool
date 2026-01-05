using Domain.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Binance.Config
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBinance(this IServiceCollection services)
        {
            services.AddHttpClient<BinanceClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.binance.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddSingleton<IExchangeClient>(sp => sp.GetRequiredService<BinanceClient>());

            return services;
        }
    }
}

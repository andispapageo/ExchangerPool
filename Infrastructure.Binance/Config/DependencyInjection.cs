using Domain.Core.Interfaces;
using Domain.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Binance.Config;

public static class DependencyInjection
{
    public const string SectionName = "Exchanges:Binance";
    public const string OptionsName = "Binance";
    public static IServiceCollection AddBinance(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://api.binance.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));
        services.AddHttpClient<BinanceClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, BinanceClient>(sp =>
            sp.GetRequiredService<BinanceClient>()));
        return services;
    }
}
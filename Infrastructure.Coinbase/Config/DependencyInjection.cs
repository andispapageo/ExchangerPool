using Domain.Core.Interfaces;
using Domain.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Coinbase.Config;

public static class DependencyInjection
{
    public const string SectionName = "Exchanges:Coinbase";
    public const string OptionsName = "Coinbase";

    public static IServiceCollection AddCoinbase(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://api.exchange.coinbase.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));
        services.AddHttpClient<CoinbaseClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, CoinbaseClient>(sp =>
            sp.GetRequiredService<CoinbaseClient>()));

        return services;
    }
}
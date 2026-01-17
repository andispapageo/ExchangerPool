using Domain.Core.Interfaces;
using Domain.Core.Options;
using Infrastructure.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Kraken.Config;
public static class DependencyInjection
{
    public const string SectionName = "Exchanges:Kraken";
    public const string OptionsName = "Kraken";

    public static IServiceCollection AddKraken(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://api.kraken.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));
        services.AddHttpClient<KrakenClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }).AddExchangeResilienceHandler("Kraken");

        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, KrakenClient>(sp =>
            sp.GetRequiredService<KrakenClient>()));

        return services;
    }
}
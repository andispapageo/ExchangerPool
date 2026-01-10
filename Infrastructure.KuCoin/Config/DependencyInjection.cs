using Domain.Core.Interfaces;
using Domain.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Infrastructure.Common.Extensions;

namespace Infrastructure.KuCoin.Config;
public static class DependencyInjection
{
    public const string SectionName = "Exchanges:KuCoin";
    public const string OptionsName = "KuCoin";

    public static IServiceCollection AddKuCoin(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://api.kucoin.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));
        services.AddHttpClient<KuCoinClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }).AddExchangeResilienceHandler("KuCoin");

        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, KuCoinClient>(sp =>
            sp.GetRequiredService<KuCoinClient>()));


        return services;
    }
}
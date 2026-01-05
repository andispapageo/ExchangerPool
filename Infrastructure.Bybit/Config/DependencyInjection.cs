using Domain.Core.Interfaces;
using Domain.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infrastructure.Bybit.Config;
public static class DependencyInjection
{
    public const string SectionName = "Exchanges:Bybit";
    public const string OptionsName = "Bybit";

    public static IServiceCollection AddBybit(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://api.bybit.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));

        services.AddHttpClient<BybitClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, BybitClient>(sp =>
    sp.GetRequiredService<BybitClient>()));
        return services;
    }
}
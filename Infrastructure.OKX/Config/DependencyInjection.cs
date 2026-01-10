using Domain.Core.Interfaces;
using Domain.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
namespace Infrastructure.OKX.Config;
public static class DependencyInjection
{
    public const string SectionName = "Exchanges:OKX";
    public const string OptionsName = "OKX";
    public static IServiceCollection AddOKX(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SectionName).Get<ExchangeOptions>()
            ?? new ExchangeOptions { BaseUrl = "https://www.okx.com/" };

        if (!options.Enabled)
            return services;

        services.Configure<ExchangeOptions>(OptionsName, configuration.GetSection(SectionName));
        services.AddHttpClient<OKXClient>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.TryAddEnumerable(ServiceDescriptor.Transient<IExchangeClient, OKXClient>(sp =>
            sp.GetRequiredService<OKXClient>()));

        return services;
    }
}
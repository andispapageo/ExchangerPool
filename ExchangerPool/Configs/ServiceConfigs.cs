using Infrastructure.Binance.Config;
using Infrastructure.Bybit.Config;
using Infrastructure.Caching.Config;
using Infrastructure.Coinbase.Config;
using Infrastructure.Kraken.Config;
using Infrastructure.KuCoin.Config;
using Infrastructure.OKX.Config;
using Application.Common.Config;
using FastEndpoints.Swagger;

namespace ExchangerPool.Configs
{
    public static class ServiceConfigs
    {
        public static IServiceCollection AddServiceConfigs(this WebApplicationBuilder builder, ILogger logger)
        {
            builder.Services.AddApplication();
            builder.Services.AddBinance(builder.Configuration);
            builder.Services.AddCoinbase(builder.Configuration);
            builder.Services.AddKraken(builder.Configuration);
            builder.Services.AddBybit(builder.Configuration);
            builder.Services.AddKuCoin(builder.Configuration);
            builder.Services.AddOKX(builder.Configuration);
            builder.Services.AddCaching();

            builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                {
                    o.DocumentSettings = s =>
                    {
                        s.Title = "Clean Architecture API";
                        s.Version = "v1";
                        s.Description = "HTTP endpoints for the Clean Architecture sample application.";
                    };
                    o.ShortSchemaNames = true;
                });


            logger.LogInformation("{Project} services registered", "Application, Binance, Coinbase, Kraken, Bybit, KuCoin, OKX, Caching");
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            return builder.Services;
        }
    }
}
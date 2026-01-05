using Application.Common.Config;
using Infrastructure.Binance.Config;
using Infrastructure.Bybit.Config;
using Infrastructure.Caching.Config;
using Infrastructure.Coinbase.Config;
using Infrastructure.Kraken.Config;
using Infrastructure.KuCoin.Config;
using Infrastructure.OKX.Config;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication();
builder.Services.AddBinance(builder.Configuration);
builder.Services.AddCoinbase(builder.Configuration);
builder.Services.AddKraken(builder.Configuration);
builder.Services.AddBybit(builder.Configuration);
builder.Services.AddKuCoin(builder.Configuration);
builder.Services.AddOKX(builder.Configuration);
builder.Services.AddCaching();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Crypto Liquidity Aggregator API",
        Version = "v1",
        Description = "Aggregates crypto prices across exchanges to find best prices and arbitrage opportunities"
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment() ||
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Crypto Liquidity Aggregator v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
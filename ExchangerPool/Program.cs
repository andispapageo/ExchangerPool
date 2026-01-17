using ExchangerPool.Configs;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

using var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
var startupLogger = loggerFactory.CreateLogger<Program>();
builder.AddServiceConfigs(startupLogger);

var app = builder.Build();
await app.UseAppMiddleware(CancellationToken.None);
await app.RunAsync();
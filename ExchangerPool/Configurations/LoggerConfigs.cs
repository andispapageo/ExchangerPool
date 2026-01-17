using Serilog;
namespace ExchangerPool.Configs;
public static class LoggerConfigs
{
    public static WebApplicationBuilder AddLoggerConfigs(this WebApplicationBuilder builder)
    {
        builder.Logging.AddSerilog(new LoggerConfiguration()
          .ReadFrom.Configuration(builder.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
          .WriteTo.Console()
          .CreateLogger());
        return builder;
    }
}

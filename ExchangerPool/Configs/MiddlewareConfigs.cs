using FastEndpoints.Swagger;

namespace ExchangerPool.Configs
{
    public static class MiddlewareConfigs
    {
        public static async Task<IApplicationBuilder> UseAppMiddleware(this WebApplication app, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                app.MapDefaultEndpoints();
                app.UseFastEndpoints();

                if (app.Environment.IsDevelopment() ||
                    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development")
                {
                    app.UseSwaggerGen();  // FastEndpoints Swagger
                    app.UseSwaggerUi(c =>
                    {
                        c.Path = "/swagger";
                        c.DocumentPath = "/swagger/v1/swagger.json";
                    });
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                return app;
            }, cancellationToken);
        }
    }
}

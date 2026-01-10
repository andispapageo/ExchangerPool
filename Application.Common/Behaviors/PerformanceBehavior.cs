using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common.Behaviors
{
    public sealed class PerformanceBehavior<TRequest, TResponse>(
     ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
     : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private const int SlowRequestThresholdMs = 500;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > SlowRequestThresholdMs)
            {
                logger.LogWarning(
                    "Long running request: {RequestName} ({ElapsedMs}ms) {@Request}",
                    typeof(TRequest).Name,
                    stopwatch.ElapsedMilliseconds,
                    request);
            }

            return response;
        }
    }
}

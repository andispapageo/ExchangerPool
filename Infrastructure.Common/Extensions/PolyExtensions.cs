using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace Infrastructure.Common.Extensions
{
    public static class PollyExtensions
    {
        public static IHttpClientBuilder AddExchangeResilienceHandler(
            this IHttpClientBuilder builder,
            string exchangeName = "Exchange")
        {
            builder.AddResilienceHandler($"{exchangeName}Resilience", (resilienceBuilder, context) =>
            {
                var logger = context.ServiceProvider.GetService<ILoggerFactory>()
                    ?.CreateLogger($"Resilience.{exchangeName}");

                // Retry: 3 attempts with exponential backoff
                resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(500),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = static args => ValueTask.FromResult(
                        args.Outcome.Result?.IsSuccessStatusCode == false ||
                        args.Outcome.Exception is HttpRequestException),
                    OnRetry = args =>
                    {
                        logger?.LogWarning(
                            "[{Exchange}] Retry attempt {Attempt} after {Delay}ms. Reason: {Reason}",
                            exchangeName,
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
                        return ValueTask.CompletedTask;
                    }
                });

                resilienceBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.8,              // Open after 80% failure rate (was 50%)
                    SamplingDuration = TimeSpan.FromSeconds(60),  // Sample over 60s (was 30s)
                    MinimumThroughput = 10,          // Need at least 10 requests (was 5)
                    BreakDuration = TimeSpan.FromSeconds(15),     // Stay open 15s (was 30s)
                    ShouldHandle = static args => ValueTask.FromResult(
                        args.Outcome.Exception is HttpRequestException or TaskCanceledException),
                    OnOpened = args =>
                    {
                        logger?.LogError(
                            "[{Exchange}] Circuit OPENED for {Duration}s. Too many failures detected.",
                            exchangeName,
                            args.BreakDuration.TotalSeconds);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = _ =>
                    {
                        logger?.LogInformation("[{Exchange}] Circuit CLOSED. Resuming normal operations.", exchangeName);
                        return ValueTask.CompletedTask;
                    },
                    OnHalfOpened = _ =>
                    {
                        logger?.LogInformation("[{Exchange}] Circuit HALF-OPEN. Testing with next request.", exchangeName);
                        return ValueTask.CompletedTask;
                    }
                });

                resilienceBuilder.AddTimeout(TimeSpan.FromSeconds(15));
            });

            return builder;
        }
    }
}

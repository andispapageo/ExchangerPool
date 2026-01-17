namespace Domain.Core.Exceptions;

public abstract class AggregatorException : Exception
{
    public string? CorrelationId { get; init; }
    public DateTime OccurredAt { get; } = DateTime.UtcNow;

    protected AggregatorException(string message) : base(message) { }
    protected AggregatorException(string message, Exception innerException) : base(message, innerException) { }
}
public sealed class NoPriceDataException : AggregatorException
{
    public string Symbol { get; }
    public IReadOnlyList<string> AttemptedExchanges { get; }

    public NoPriceDataException(string symbol, IEnumerable<string> attemptedExchanges)
        : base($"No price data available for symbol '{symbol}' from any exchange.")
    {
        Symbol = symbol;
        AttemptedExchanges = attemptedExchanges.ToList().AsReadOnly();
    }
}

public sealed class ExchangeApiException : AggregatorException
{
    public string ExchangeName { get; }
    public string? Endpoint { get; init; }
    public int? StatusCode { get; init; }

    public ExchangeApiException(string exchangeName, string message, Exception? innerException = null)
        : base($"[{exchangeName}] {message}", innerException!)
    {
        ExchangeName = exchangeName;
    }
}

public sealed class AggregationException : AggregatorException
{
    public IReadOnlyDictionary<string, Exception> ExchangeErrors { get; }
    public int SuccessCount { get; }
    public int FailureCount { get; }

    public AggregationException(
        string message,
        IDictionary<string, Exception> exchangeErrors,
        int successCount)
        : base(message)
    {
        ExchangeErrors = exchangeErrors.AsReadOnly();
        SuccessCount = successCount;
        FailureCount = exchangeErrors.Count;
    }
}
public sealed class PartialResultException : AggregatorException
{
    public IReadOnlyList<string> SuccessfulExchanges { get; }
    public IReadOnlyDictionary<string, string> FailedExchanges { get; }
    public PartialResultException(
        IEnumerable<string> successfulExchanges,
        IDictionary<string, string> failedExchanges)
        : base($"Partial result: {successfulExchanges.Count()} succeeded, {failedExchanges.Count} failed.")
    {
        SuccessfulExchanges = successfulExchanges.ToList().AsReadOnly();
        FailedExchanges = failedExchanges.AsReadOnly();
    }
}
public sealed class RateLimitedException : AggregatorException
{
    public string ExchangeName { get; }
    public TimeSpan? RetryAfter { get; }
    public RateLimitedException(string exchangeName, TimeSpan? retryAfter = null)
        : base($"Rate limited by {exchangeName}. Retry after: {retryAfter?.TotalSeconds ?? 0}s")
    {
        ExchangeName = exchangeName;
        RetryAfter = retryAfter;
    }
}
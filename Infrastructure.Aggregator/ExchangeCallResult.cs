namespace Infrastructure.Aggregator;

public readonly record struct ExchangeCallResult<T>
{
    public string ExchangeName { get; init; }
    public T? Data { get; init; }
    public bool IsSuccess { get; init; }
    public ExchangeErrorType ErrorType { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
    public TimeSpan Duration { get; init; }
    public static ExchangeCallResult<T> Success(string exchangeName, T data, TimeSpan duration) => new()
    {
        ExchangeName = exchangeName,
        Data = data,
        IsSuccess = true,
        ErrorType = ExchangeErrorType.None,
        Duration = duration
    };

    public static ExchangeCallResult<T> Failure(
        string exchangeName,
        ExchangeErrorType errorType,
        string errorMessage,
        Exception? exception = null,
        TimeSpan duration = default) => new()
    {
        ExchangeName = exchangeName,
        Data = default,
        IsSuccess = false,
        ErrorType = errorType,
        ErrorMessage = errorMessage,
        Exception = exception,
        Duration = duration
    };
}

public enum ExchangeErrorType
{
    None,
    Timeout,
    RateLimited,
    NetworkError,
    InvalidResponse,
    SymbolNotFound,
    ServiceUnavailable,
    Unknown,
    Cancelled
}
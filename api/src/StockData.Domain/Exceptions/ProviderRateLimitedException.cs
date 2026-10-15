namespace StockData.Domain.Exceptions;
/// <summary>Raised when a provider rate-limits a request.</summary>
public sealed class ProviderRateLimitedException(TimeSpan? retryAfter) : StockDataException("Provider rate limited the request")
{
    /// <summary>Suggested retry delay.</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

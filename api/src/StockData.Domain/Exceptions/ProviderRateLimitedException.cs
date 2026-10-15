namespace StockData.Domain.Exceptions;
/// <summary>Raised when a provider rate-limits a request.</summary>
public sealed class ProviderRateLimitedException(TimeSpan? retryAfter = null) : StockDataException("Provider rate limited the request")
{
    // TODO: reject non-positive retryAfter values with ArgumentOutOfRangeException.
    /// <summary>Suggested retry delay.</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

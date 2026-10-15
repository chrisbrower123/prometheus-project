namespace StockData.Domain.Exceptions;
/// <summary>Raised when a provider rate-limits a request.</summary>
public sealed class ProviderRateLimitedException : StockDataException
{
    public ProviderRateLimitedException(TimeSpan? retryAfter = null) : base("Provider rate limited the request")
    {
        if (retryAfter <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(retryAfter), retryAfter, "Retry delay must be positive.");
        }

        RetryAfter = retryAfter;
    }

    /// <summary>Suggested retry delay.</summary>
    public TimeSpan? RetryAfter { get; }
}

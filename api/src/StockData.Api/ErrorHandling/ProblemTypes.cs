namespace StockData.Api.ErrorHandling;

public static class ProblemTypes
{
    public const string InvalidSymbol = "urn:stockdata:problem:invalid-symbol";
    public const string SymbolNotFound = "urn:stockdata:problem:symbol-not-found";
    public const string NoDataAvailable = "urn:stockdata:problem:no-data-available";
    public const string RateLimited = "urn:stockdata:problem:rate-limited";
    public const string ProviderUnavailable = "urn:stockdata:problem:provider-unavailable";
    public const string Unexpected = "urn:stockdata:problem:unexpected";
}

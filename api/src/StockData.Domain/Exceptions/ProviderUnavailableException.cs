namespace StockData.Domain.Exceptions;
/// <summary>Raised when a provider is unavailable.</summary>
public sealed class ProviderUnavailableException(string message, Exception? innerException = null) : StockDataException(message, innerException!);

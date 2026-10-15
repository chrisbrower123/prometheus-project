namespace StockData.Domain.Exceptions;

/// <summary>Base exception for stock data failures.</summary>
public abstract class StockDataException : Exception
{
    /// <summary>Initializes the exception.</summary>
    protected StockDataException() { }
    /// <summary>Initializes the exception.</summary>
    protected StockDataException(string message) : base(message) { }
    /// <summary>Initializes the exception.</summary>
    protected StockDataException(string message, Exception innerException) : base(message, innerException) { }
}
